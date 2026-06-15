package com.mitube.core.player

import android.util.Log
import android.webkit.JavascriptInterface
import com.google.gson.Gson
import com.mitube.core.models.Channel

class JsBridge(private val listener: Listener) {

    interface Listener {
        fun onPlayerLoaded()
        fun onPlayerError(error: String)
        fun onPlayerTimeout()
        fun onConsoleLog(level: String, message: String)
    }

    @JavascriptInterface
    fun postMessage(json: String) {
        try {
            val msg = Gson().fromJson(json, Map::class.java) as Map<String, Any>
            val action = msg["action"] as? String ?: return

            when (action) {
                "loaded" -> listener.onPlayerLoaded()
                "loadError" -> listener.onPlayerError(msg["message"] as? String ?: "Unknown error")
                "error" -> listener.onPlayerError(msg["message"] as? String ?: "Unknown error")
                "timeout" -> listener.onPlayerTimeout()
                "console" -> {
                    val level = msg["level"] as? String ?: "log"
                    val message = msg["message"] as? String ?: ""
                    listener.onConsoleLog(level, message)
                }
            }
        } catch (e: Exception) {
            Log.w(TAG, "JsBridge parse error: ${e.message}")
        }
    }

    companion object {
        private const val TAG = "JsBridge"
    }
}
