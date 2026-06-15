package com.mitube.mobile.ui

import android.content.Intent
import android.content.pm.ActivityInfo
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.Log
import android.view.MotionEvent
import android.view.View
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import android.view.WindowManager
import android.widget.ImageView
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.commit
import com.mitube.core.models.Channel
import com.mitube.core.player.PlayerWebViewFragment
import com.mitube.core.proxy.ProxyServer
import com.mitube.mobile.MiTubeMobileApp
import com.mitube.mobile.R

class PlayerActivity : AppCompatActivity() {

    private var proxyServer: ProxyServer? = null
    private var playerFragment: PlayerWebViewFragment? = null
    private var overlayVisible = true
    private val overlayHandler = Handler(Looper.getMainLooper())
    private val overlayHideRunnable = Runnable { hideOverlay() }
    private var currentChannel: Channel? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        requestedOrientation = ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE
        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        window.decorView.systemUiVisibility = (
            View.SYSTEM_UI_FLAG_FULLSCREEN or
            View.SYSTEM_UI_FLAG_HIDE_NAVIGATION or
            View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
        )
        supportActionBar?.hide()

        setContentView(R.layout.activity_player)

        val channelName = findViewById<TextView>(R.id.playerChannelName)
        val headersJson = intent.getStringExtra("channel_headers")
        val headers: Map<String, String>? = if (headersJson != null && headersJson != "null") {
            try {
                Gson().fromJson(headersJson, object : TypeToken<Map<String, String>>() {}.type)
            } catch (_: Exception) { null }
        } else null
        val channel = Channel(
            name = intent.getStringExtra("channel_name") ?: "",
            url = intent.getStringExtra("channel_url") ?: "",
            type = intent.getStringExtra("channel_type") ?: "",
            drm_license_uri = intent.getStringExtra("channel_drm_license_uri") ?: "",
            icono = intent.getStringExtra("channel_icono") ?: "",
            headers = headers
        )
        Log.d("PlayerActivity", "Channel: $channel.url, type=$channel.type, hasHeaders=${headers != null}")
        channelName.text = channel.name
        currentChannel = channel

        findViewById<ImageView>(R.id.playerBackBtn).setOnClickListener {
            saveLastChannel()
            finish()
        }

        proxyServer = ProxyServer(ProxyServer.findAvailablePort()).apply {
            try {
                start()
                Log.d("PlayerActivity", "Proxy started on port $listeningPort")
            } catch (e: Exception) {
                Log.e("PlayerActivity", "Failed to start proxy", e)
            }
        }

        PlayerWebViewFragment().apply {
            configure(proxyServer!!)
        }.also { fragment ->
            playerFragment = fragment
            supportFragmentManager.commit {
                replace(R.id.playerContainer, fragment)
            }
            fragment.loadChannel(channel)
        }

        overlayHandler.postDelayed(overlayHideRunnable, 4000L)
    }

    override fun dispatchTouchEvent(event: MotionEvent?): Boolean {
        if (event?.action == MotionEvent.ACTION_UP) {
            toggleOverlay()
        }
        return super.dispatchTouchEvent(event)
    }

    private fun toggleOverlay() {
        if (overlayVisible) hideOverlay()
        else showOverlay()
    }

    private fun showOverlay() {
        findViewById<View>(R.id.playerOverlay).visibility = View.VISIBLE
        overlayVisible = true
        overlayHandler.removeCallbacks(overlayHideRunnable)
        overlayHandler.postDelayed(overlayHideRunnable, 4000L)
    }

    private fun hideOverlay() {
        findViewById<View>(R.id.playerOverlay).visibility = View.GONE
        overlayVisible = false
        overlayHandler.removeCallbacks(overlayHideRunnable)
        playerFragment?.forceHideControls()
    }

    override fun onDestroy() {
        saveLastChannel()
        overlayHandler.removeCallbacks(overlayHideRunnable)
        proxyServer?.stop()
        super.onDestroy()
    }

    private fun saveLastChannel() {
        currentChannel?.let {
            MiTubeMobileApp.instance.sessionManager.lastChannelName = it.name
        }
    }
}
