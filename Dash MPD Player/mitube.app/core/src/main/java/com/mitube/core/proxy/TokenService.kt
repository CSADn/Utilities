package com.mitube.core.proxy

import com.google.gson.JsonParser
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import com.mitube.core.api.ApiClient
import java.util.concurrent.TimeUnit

class TokenService(private val httpClient: OkHttpClient? = null) {

    private val client: OkHttpClient by lazy {
        httpClient ?: OkHttpClient.Builder()
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .followRedirects(true)
            .build()
    }

    suspend fun getCdnTokenAsync(mpdUrl: String): String = withContext(Dispatchers.IO) {
        val encodedUrl = java.net.URLEncoder.encode(mpdUrl, "UTF-8")
        val backendUrl = ApiClient.getBaseUrl() + "api/proxy/cdn-token?url=$encodedUrl"

        val requestBuilder = Request.Builder()
            .url(backendUrl)
            .get()

        val token = ApiClient.getAuthToken()
        if (token != null) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }

        val response = client.newCall(requestBuilder.build()).execute()
        if (!response.isSuccessful) {
            throw RuntimeException("CDN token request failed: ${response.code}")
        }

        val json = response.body?.string() ?: throw RuntimeException("Empty CDN token response")
        val obj = JsonParser.parseString(json).asJsonObject
        obj.get("cdnToken").asString
    }
}
