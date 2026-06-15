package com.mitube.tv.ui

import android.content.Intent
import android.content.pm.ActivityInfo
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.KeyEvent
import android.view.View
import android.view.WindowManager
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.mitube.core.models.Channel
import com.mitube.core.player.PlayerWebViewFragment
import com.mitube.core.proxy.ProxyServer
import com.mitube.tv.R

class PlayerActivity : AppCompatActivity() {

    private var proxyServer: ProxyServer? = null
    private var playerFragment: PlayerWebViewFragment? = null
    private var overlayVisible = true
    private val overlayHandler = Handler(Looper.getMainLooper())
    private val overlayHideRunnable = Runnable { hideOverlay() }

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

        val channelNameText = findViewById<TextView>(R.id.playerChannelName)
        val channel = Channel(
            name = intent.getStringExtra("channel_name") ?: "",
            url = intent.getStringExtra("channel_url") ?: "",
            type = intent.getStringExtra("channel_type") ?: "",
            drm_license_uri = intent.getStringExtra("channel_drm_license_uri") ?: "",
            icono = intent.getStringExtra("channel_icono") ?: ""
        )
        channelNameText.text = channel.name

        proxyServer = ProxyServer(ProxyServer.findAvailablePort())

        PlayerWebViewFragment().apply {
            configure(proxyServer!!)
        }.also { fragment ->
            playerFragment = fragment
            supportFragmentManager.beginTransaction()
                .replace(R.id.playerContainer, fragment)
                .commitAllowingStateLoss()
            fragment.loadChannel(channel)
        }

        overlayHandler.postDelayed(overlayHideRunnable, 4000L)
    }

    override fun dispatchKeyEvent(event: KeyEvent): Boolean {
        if (event.action == KeyEvent.ACTION_DOWN &&
            (event.keyCode == KeyEvent.KEYCODE_DPAD_CENTER || event.keyCode == KeyEvent.KEYCODE_ENTER)
        ) {
            toggleOverlay()
            return true
        }
        return super.dispatchKeyEvent(event)
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
        overlayHandler.removeCallbacks(overlayHideRunnable)
        proxyServer?.stop()
        super.onDestroy()
    }
}
