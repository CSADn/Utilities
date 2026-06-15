package com.mitube.core.proxy

import android.util.Log
import com.mitube.core.models.DrmInfo
import fi.iki.elonen.NanoHTTPD
import fi.iki.elonen.NanoHTTPD.Response
import kotlinx.coroutines.runBlocking
import okhttp3.CookieJar
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
import java.io.ByteArrayInputStream
import java.net.URLDecoder
import java.net.URLEncoder
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicInteger

class ProxyServer(port: Int) : NanoHTTPD(port) {

    private val serverPort = port

    private val userAgent = "Mozilla/5.0 (Linux; Android 14; SM-S928B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.6422.165 Mobile Safari/537.36"

    private val httpClient = OkHttpClient.Builder()
        .connectTimeout(30, TimeUnit.SECONDS)
        .readTimeout(30, TimeUnit.SECONDS)
        .followRedirects(true)
        .followSslRedirects(true)
        .cookieJar(CookieJar.NO_COOKIES)
        .addInterceptor { chain ->
            val req = chain.request().newBuilder()
                .header("User-Agent", chain.request().header("User-Agent") ?: userAgent)
                .build()
            chain.proceed(req)
        }
        .build()

    private val segmentClient = OkHttpClient.Builder()
        .connectTimeout(60, TimeUnit.SECONDS)
        .readTimeout(120, TimeUnit.SECONDS)
        .followRedirects(false)
        .followSslRedirects(false)
        .cookieJar(CookieJar.NO_COOKIES)
        .addInterceptor { chain ->
            val req = chain.request().newBuilder()
                .header("User-Agent", chain.request().header("User-Agent") ?: userAgent)
                .build()
            chain.proceed(req)
        }
        .build()

    private val tokenService = TokenService(httpClient)
    private val mpdRewriter = MpdRewriter("http://localhost:$serverPort")

    @Volatile
    private var currentMpdUrl: String? = null
    @Volatile
    private var currentHeaders: Map<String, String>? = null

    private val sessionEntries = ConcurrentHashMap<String, SessionEntry>()
    private val sessionCounter = AtomicInteger(0)
    @Volatile
    var playerHtml: String = ""
        private set

    data class SessionEntry(val baseUrl: String, val createdAt: Long = System.currentTimeMillis())

    fun setPlayerHtml(html: String) {
        playerHtml = html
    }

    fun setChannelConfig(mpdUrl: String, headers: Map<String, String>?) {
        currentMpdUrl = mpdUrl
        currentHeaders = headers
        Log.d(TAG, "Channel config set: $mpdUrl, headers: ${headers != null}")
    }

    override fun serve(session: IHTTPSession): Response {
        return try {
            val path = session.uri
            val method = session.method
            val params = session.parms
            val isHead = method == Method.HEAD

            Log.d(TAG, "=> $method $path")

            when {
                path == "/player" || path == "/player.html" -> servePlayer()
                path == "/manifest" -> handleManifest(params, isHead)
                path.startsWith("/segment/") -> handleSegment(path, params)
                path == "/license" -> handleLicense(session, params)
                path == "/fetch" -> handleFetch(session, params, isHead)
                method == Method.OPTIONS -> newFixedLengthResponse(
                    Response.Status.NO_CONTENT, MIME_PLAINTEXT, ""
                ).also { addCorsHeaders(it) }
                else -> newFixedLengthResponse(
                    Response.Status.NOT_FOUND, MIME_PLAINTEXT, "Not found"
                )
            }
        } catch (e: Exception) {
            Log.e(TAG, "Error processing request: ${session.uri}", e)
            newFixedLengthResponse(
                Response.Status.INTERNAL_ERROR, MIME_PLAINTEXT, "Error: ${e.message}"
            )
        }
    }

    private fun servePlayer(): Response {
        return newFixedLengthResponse(
            Response.Status.OK, "text/html; charset=utf-8", playerHtml
        ).also { addCorsHeaders(it) }
    }

    private fun handleManifest(params: Map<String, String>, isHead: Boolean): Response {
        val mpdUrl = params["url"] ?: return newFixedLengthResponse(
            Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Missing url parameter"
        )

        val (mpdContent, baseUrl) = runBlocking {
            if (currentHeaders != null) {
                fetchSignedManifest(mpdUrl, currentHeaders!!)
            } else {
                fetchDirectManifest(mpdUrl)
            }
        }

        cleanStaleSessions()
        val token = sessionCounter.incrementAndGet().toString(16)
        sessionEntries[token] = SessionEntry(baseUrl)

        val rewritten = mpdRewriter.rewrite(mpdContent, token)
        val finalContent = if (rewritten.trimStart().startsWith("#EXTM3U")) {
            rewriteHlsPlaylist(rewritten, mpdUrl)
        } else rewritten

        val isHls = finalContent.trimStart().startsWith("#EXTM3U")
        val contentType = if (isHls) "application/vnd.apple.mpegURL; charset=utf-8"
        else "application/dash+xml; charset=utf-8"

        Log.d(TAG, "MPD rewritten (${finalContent.length} chars), token=$token")
        val response = newFixedLengthResponse(
            Response.Status.OK, contentType, finalContent
        )
        if (isHead) {
            return response.also { addCorsHeaders(it) }
        }
        return response.also { addCorsHeaders(it) }
    }

    private fun handleSegment(path: String, params: Map<String, String>): Response {
        val prefix = "/segment/"
        if (!path.startsWith(prefix)) return newFixedLengthResponse(
            Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Invalid segment path"
        )

        val rest = path.removePrefix(prefix)
        val slashIdx = rest.indexOf('/')
        if (slashIdx < 0) return newFixedLengthResponse(
            Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Invalid segment format"
        )

        val token = rest.substring(0, slashIdx)
        val filePath = rest.substring(slashIdx + 1)
        val entry = sessionEntries[token]
            ?: return newFixedLengthResponse(
                Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Invalid session token"
            )

        val segmentUrl = entry.baseUrl.trimEnd('/') + "/" + filePath.trimStart('/')
        Log.d(TAG, "Segment request: $segmentUrl")

        val requestBuilder = Request.Builder().url(segmentUrl).get()
        currentHeaders?.forEach { (key, value) ->
            if (key.isNotBlank() && value.isNotBlank() &&
                !key.equals("Content-Type", ignoreCase = true)
            ) requestBuilder.addHeader(key, value)
        }

        try {
            val response = segmentClient.newCall(requestBuilder.build()).execute()
            if (!response.isSuccessful) {
                return newFixedLengthResponse(
                    Response.Status.lookup(response.code),
                    response.body?.contentType()?.toString() ?: MIME_PLAINTEXT,
                    ""
                )
            }

            val body = response.body ?: return newFixedLengthResponse(
                Response.Status.INTERNAL_ERROR, MIME_PLAINTEXT, "Empty response"
            )

            val bytes = body.bytes()
            val contentType = response.header("Content-Type") ?: "application/octet-stream"
            Log.d(TAG, "Segment served (${bytes.size} bytes): $segmentUrl")
            return newFixedLengthResponse(Response.Status.OK, contentType, ByteArrayInputStream(bytes), bytes.size.toLong())
                .also { addCorsHeaders(it) }
        } catch (e: Exception) {
            Log.w(TAG, "Segment error for $segmentUrl", e)
            return newFixedLengthResponse(Response.Status.lookup(502), MIME_PLAINTEXT, "Upstream error")
        }
    }

    private fun handleLicense(session: IHTTPSession, params: Map<String, String>): Response {
        val licenseUrl = params["url"] ?: return newFixedLengthResponse(
            Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Missing url parameter"
        )

        return try {
            val contentLength = session.headers["content-length"]?.toIntOrNull() ?: 0
            val body = ByteArray(contentLength)
            session.inputStream?.read(body)

            Log.d(TAG, "License request for: $licenseUrl (${body.size} bytes)")

            val requestBuilder = Request.Builder()
                .url(licenseUrl)
                .post(RequestBody.create(null, body))
                .addHeader("Content-Type", "application/octet-stream")

            currentHeaders?.forEach { (key, value) ->
                if (key.isNotBlank() && value.isNotBlank() &&
                    !key.equals("Content-Type", ignoreCase = true)
                ) requestBuilder.addHeader(key, value)
            }
            requestBuilder.addHeader("Origin", "https://www.amazon.com")
            requestBuilder.addHeader("Referer", "https://www.amazon.com/")

            val response = httpClient.newCall(requestBuilder.build()).execute()
            val licenseBytes = response.body?.bytes() ?: ByteArray(0)
            val contentType = response.header("Content-Type") ?: "application/octet-stream"

            Log.d(TAG, "License response: ${response.code}, ${licenseBytes.size} bytes")
            val nanoResponse = newFixedLengthResponse(
                Response.Status.lookup(response.code),
                contentType,
                ByteArrayInputStream(licenseBytes),
                licenseBytes.size.toLong()
            )
            addCorsHeaders(nanoResponse)
            nanoResponse
        } catch (e: Exception) {
            Log.w(TAG, "License proxy error for $licenseUrl", e)
            newFixedLengthResponse(Response.Status.lookup(502), MIME_PLAINTEXT, "Proxy license error")
        }
    }

    private fun handleFetch(session: IHTTPSession, params: Map<String, String>, isHead: Boolean): Response {
        val url = params["url"] ?: return newFixedLengthResponse(
            Response.Status.BAD_REQUEST, MIME_PLAINTEXT, "Missing url parameter"
        )
        Log.d(TAG, "Fetch: $url")

        val requestBuilder = Request.Builder().url(url).get()
        currentHeaders?.forEach { (key, value) ->
            if (key.isNotBlank() && value.isNotBlank() &&
                !key.equals("Content-Type", ignoreCase = true)
            ) requestBuilder.addHeader(key, value)
        }

        return try {
            val response = segmentClient.newCall(requestBuilder.build()).execute()
            if (!response.isSuccessful) {
                return newFixedLengthResponse(
                    Response.Status.lookup(response.code), MIME_PLAINTEXT, "Upstream error: ${response.code}"
                )
            }

            val body = response.body ?: return newFixedLengthResponse(
                Response.Status.INTERNAL_ERROR, MIME_PLAINTEXT, "Empty response"
            )

            var bytes = body.bytes()
            var contentType = detectContentType(url, bytes)

            // Rewrite HLS playlists
            if (contentType.startsWith("application/vnd.apple.mpegURL") ||
                contentType.startsWith("application/x-mpegURL")) {
                val text = String(bytes, Charsets.UTF_8)
                val rewritten = rewriteHlsPlaylist(text, url, false)
                bytes = rewritten.toByteArray(Charsets.UTF_8)
            }

            Log.d(TAG, "Fetch served (${bytes.size} bytes): $url")
            newFixedLengthResponse(Response.Status.OK, contentType, ByteArrayInputStream(bytes), bytes.size.toLong())
                .also { addCorsHeaders(it) }
        } catch (e: Exception) {
            Log.w(TAG, "Fetch error for $url", e)
            newFixedLengthResponse(Response.Status.lookup(502), MIME_PLAINTEXT, "Upstream error")
        }
    }

    // --- Internal helpers ---

    private fun proxyBaseUrl() = "http://localhost:$serverPort"

    private fun addCorsHeaders(response: Response) {
        response.addHeader("Access-Control-Allow-Origin", "*")
        response.addHeader("Access-Control-Allow-Methods", "GET, HEAD, POST, OPTIONS")
        response.addHeader("Access-Control-Allow-Headers", "*")
    }

    private fun cleanStaleSessions() {
        if (sessionEntries.size < 50) return
        val cutoff = System.currentTimeMillis() - 300_000L // 5 min
        sessionEntries.entries.removeAll { it.value.createdAt < cutoff }
    }

    private suspend fun fetchSignedManifest(
        mpdUrl: String, headers: Map<String, String>
    ): Pair<String, String> {
        val cdnToken = tokenService.getCdnTokenAsync(mpdUrl)

        val separator = if (mpdUrl.contains("?")) "&" else "?"
        val signedUrl = "$mpdUrl${separator}cdntoken=$cdnToken"
        Log.d(TAG, "Signed URL built")

        val mpdResponse = sendManifestRequest(signedUrl, headers)
        val mpdContent = mpdResponse.body?.string() ?: throw RuntimeException("Empty manifest")
        val resolvedUrl = mpdResponse.networkResponse?.request?.url?.toString() ?: mpdUrl

        val slashIdx = resolvedUrl.lastIndexOf('/')
        val baseUrl = if (slashIdx >= 0) resolvedUrl.substring(0, slashIdx + 1) else resolvedUrl
        return Pair(mpdContent, baseUrl)
    }

    private suspend fun fetchDirectManifest(mpdUrl: String): Pair<String, String> {
        Log.d(TAG, "Fetching MPD directly: $mpdUrl")
        val request = Request.Builder().url(mpdUrl).get().build()
        val response = httpClient.newCall(request).execute()
        val body = response.body?.string() ?: ""
        if (!response.isSuccessful) {
            Log.w(TAG, "Direct manifest fetch failed: HTTP ${response.code} for $mpdUrl, body: ${body.take(200)}")
            throw RuntimeException("Manifest request failed: HTTP ${response.code}")
        }
        if (body.isEmpty()) throw RuntimeException("Empty manifest")

        val slashIdx = mpdUrl.lastIndexOf('/')
        val baseUrl = if (slashIdx >= 0) mpdUrl.substring(0, slashIdx + 1) else mpdUrl
        return Pair(body, baseUrl)
    }

    private fun sendManifestRequest(
        signedUrl: String, headers: Map<String, String>
    ): okhttp3.Response {
        val requestBuilder = Request.Builder().url(signedUrl).get()
        headers.forEach { (key, value) ->
            if (key.isNotBlank() && value.isNotBlank() &&
                !key.equals("Content-Type", ignoreCase = true)
            ) requestBuilder.addHeader(key, value)
        }

        val response = httpClient.newCall(requestBuilder.build()).execute()

        if (!response.isSuccessful) {
            throw RuntimeException("Manifest request failed: ${response.code}")
        }
        return response
    }

    private fun detectContentType(url: String, content: ByteArray): String {
        val path = if (url.contains('?')) url.substring(0, url.indexOf('?')) else url
        val ext = path.substringAfterLast('.', "").lowercase()
        return when (ext) {
            "m3u8" -> "application/vnd.apple.mpegURL; charset=utf-8"
            "ts" -> "video/mp2t"
            "mp4" -> "video/mp4"
            "m4s" -> "application/octet-stream"
            "aac" -> "audio/aac"
            "m4a" -> "audio/mp4"
            "vtt" -> "text/vtt; charset=utf-8"
            "key" -> "application/octet-stream"
            else -> "application/octet-stream"
        }
    }

    private fun rewriteHlsPlaylist(content: String, playlistUrl: String, rewriteMediaUrls: Boolean = true): String {
        val baseDir = playlistUrl.substring(0, playlistUrl.lastIndexOf('/') + 1)
        val proxyBase = proxyBaseUrl()
        return content.lines().joinToString("\n") { line ->
            val trimmed = line.trim()
            when {
                trimmed.isBlank() -> line
                rewriteMediaUrls && (trimmed.startsWith("http://") || trimmed.startsWith("https://")) ->
                    "$proxyBase/fetch?url=${URLEncoder.encode(trimmed, "UTF-8")}"
                trimmed.contains("URI=\"") -> rewriteUriAttribute(line, '"', baseDir, proxyBase)
                trimmed.contains("URI='") -> rewriteUriAttribute(line, '\'', baseDir, proxyBase)
                rewriteMediaUrls && !trimmed.startsWith("#") -> {
                    try {
                        val absolute = java.net.URI(baseDir).resolve(trimmed).toString()
                        "$proxyBase/fetch?url=${URLEncoder.encode(absolute, "UTF-8")}"
                    } catch (_: Exception) { line }
                }
                else -> line
            }
        }
    }

    private fun detectDrmAsync(mpdUrl: String, headers: Map<String, String>?): DrmInfo? {
        return try {
            val requestBuilder = Request.Builder().url(mpdUrl).get()
            headers?.forEach { (key, value) ->
                if (key.isNotBlank() && !key.equals("Content-Type", ignoreCase = true))
                    requestBuilder.addHeader(key, value)
            }
            val response = httpClient.newCall(requestBuilder.build()).execute()
            val mpdContent = response.body?.string() ?: return null
            mpdRewriter.detectDrm(mpdContent)
        } catch (_: Exception) { null }
    }

    private fun rewriteUriAttribute(line: String, quote: Char, baseDir: String, proxyBase: String): String {
        val attrPattern = "URI=$quote"
        val startIdx = line.indexOf(attrPattern)
        if (startIdx < 0) return line
        val contentStart = startIdx + attrPattern.length
        val endIdx = line.indexOf(quote, contentStart)
        if (endIdx < 0) return line

        val originalUri = line.substring(contentStart, endIdx)
        if (originalUri.contains("/fetch?url=")) return line

        val absoluteUri = if (originalUri.startsWith("http://") || originalUri.startsWith("https://")) {
            originalUri
        } else {
            try { java.net.URI(baseDir).resolve(originalUri).toString() }
            catch (_: Exception) { return line }
        }

        val proxied = "$proxyBase/fetch?url=${URLEncoder.encode(absoluteUri, "UTF-8")}"
        return line.substring(0, contentStart) + proxied + line.substring(endIdx)
    }

    companion object {
        private const val TAG = "ProxyServer"

        fun findAvailablePort(): Int {
            val socket = java.net.ServerSocket(0)
            val port = socket.localPort
            socket.close()
            return port
        }
    }
}
