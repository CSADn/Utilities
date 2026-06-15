package com.mitube.core.player

import android.annotation.SuppressLint
import android.graphics.Bitmap
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.webkit.WebChromeClient
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.fragment.app.Fragment
import com.google.gson.Gson
import com.mitube.core.models.Channel
import com.mitube.core.proxy.ProxyServer

class PlayerWebViewFragment : Fragment() {

    private var webView: WebView? = null
    private var proxyServer: ProxyServer? = null
    private var currentChannel: Channel? = null

    private val jsBridge = JsBridge(object : JsBridge.Listener {
        override fun onPlayerLoaded() {
            Log.d(TAG, "Player loaded")
        }

        override fun onPlayerError(error: String) {
            Log.w(TAG, "Player error: $error")
        }

        override fun onPlayerTimeout() {
            Log.w(TAG, "Player timeout")
        }

        override fun onConsoleLog(level: String, message: String) {
            when (level) {
                "error" -> Log.e(TAG, message)
                "warn" -> Log.w(TAG, message)
                else -> Log.d(TAG, message)
            }
        }
    })

    fun configure(proxy: ProxyServer) {
        proxyServer = proxy
    }

    fun forceHideControls() {
        webView?.evaluateJavascript(
            "controlsVisible=false;document.getElementById('controls').classList.remove('show','visible');if(controlsTimer){clearTimeout(controlsTimer);controlsTimer=null;}",
            null
        )
    }

    fun loadChannel(channel: Channel) {
        val proxyPort = proxyServer?.listeningPort ?: 0
        val clearkey = channel.parseClearkeyLicense()
        val enrichedChannel = if (proxyPort > 0) {
            channel.copy(
                manifestUrl = "http://localhost:$proxyPort/manifest?url=${java.net.URLEncoder.encode(channel.url, "UTF-8")}",
                licenseProxyUrl = if (channel.drm_license_uri.isNotBlank())
                    "http://localhost:$proxyPort/license?url=${java.net.URLEncoder.encode(channel.drm_license_uri, "UTF-8")}"
                else "",
                keyId = clearkey?.keyId ?: "",
                key = clearkey?.key ?: ""
            )
        } else {
            channel.copy(
                manifestUrl = channel.url,
                licenseProxyUrl = channel.drm_license_uri,
                keyId = clearkey?.keyId ?: "",
                key = clearkey?.key ?: ""
            )
        }

        currentChannel = enrichedChannel
        proxyServer?.setChannelConfig(enrichedChannel.url, enrichedChannel.headers)
        val json = Gson().toJson(enrichedChannel)
        webView?.evaluateJavascript("loadChannel($json)", null)
    }

    @SuppressLint("SetJavaScriptEnabled")
    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?
    ): View? {
        val context = requireContext()
        val proxy = proxyServer

        if (proxy != null && proxy.playerHtml.isEmpty()) {
            try {
                val html = context.assets.open("player.html")
                    .bufferedReader().use { it.readText() }
                proxy.setPlayerHtml(html)
                Log.d(TAG, "Player HTML loaded from assets (${html.length} chars)")
            } catch (e: Exception) {
                Log.e(TAG, "Failed to load player.html from assets", e)
            }
        }

        val wv = WebView(context).apply {
            settings.apply {
                javaScriptEnabled = true
                domStorageEnabled = true
                allowFileAccess = false
                allowContentAccess = true
                mediaPlaybackRequiresUserGesture = false
                mixedContentMode = android.webkit.WebSettings.MIXED_CONTENT_ALWAYS_ALLOW
                useWideViewPort = true
                loadWithOverviewMode = true
            }

            webViewClient = object : WebViewClient() {
                override fun onPageStarted(view: WebView?, url: String?, favicon: Bitmap?) {
                    Log.d(TAG, "Page loading: $url")
                }

                override fun onPageFinished(view: WebView?, url: String?) {
                    Log.d(TAG, "Page loaded: $url")
                    currentChannel?.let { channel ->
                        val json = Gson().toJson(channel)
                        view?.evaluateJavascript("loadChannel($json)", null)
                    }
                }
            }

            webChromeClient = object : WebChromeClient() {
                override fun onShowCustomView(view: View?, callback: CustomViewCallback?) {}
                override fun onHideCustomView() {}
            }

            addJavascriptInterface(jsBridge, "Android")

            val proxyPort = proxy?.listeningPort ?: 0
            if (proxyPort > 0) {
                loadUrl("http://localhost:$proxyPort/player")
            }
        }

        webView = wv
        return wv
    }

    override fun onDestroyView() {
        webView?.destroy()
        webView = null
        super.onDestroyView()
    }

    companion object {
        private const val TAG = "PlayerWebViewFragment"
    }
}
