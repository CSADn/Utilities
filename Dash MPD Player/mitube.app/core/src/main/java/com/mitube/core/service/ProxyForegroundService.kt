package com.mitube.core.service

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Intent
import android.os.Build
import android.os.IBinder
import android.util.Log
import androidx.core.app.NotificationCompat
import com.mitube.core.proxy.ProxyServer

class ProxyForegroundService : Service() {

    private var proxyServer: ProxyServer? = null

    override fun onCreate() {
        super.onCreate()
        createNotificationChannel()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        when (intent?.action) {
            ACTION_START -> {
                val port = intent.getIntExtra(EXTRA_PORT, 0)
                val html = intent.getStringExtra(EXTRA_HTML) ?: ""
                startProxy(port, html)
                val notification = buildNotification()
                startForeground(NOTIFICATION_ID, notification)
            }
            ACTION_STOP -> {
                stopProxy()
                stopForeground(STOP_FOREGROUND_REMOVE)
                stopSelf()
            }
        }
        return START_STICKY
    }

    private fun startProxy(port: Int, html: String) {
        try {
            val actualPort = if (port <= 0) ProxyServer.findAvailablePort() else port
            proxyServer = ProxyServer(actualPort).apply {
                setPlayerHtml(html)
                start()
            }
            Log.i(TAG, "Proxy started on port ${proxyServer?.listeningPort}")
        } catch (e: Exception) {
            Log.e(TAG, "Failed to start proxy", e)
        }
    }

    private fun stopProxy() {
        proxyServer?.stop()
        proxyServer = null
        Log.i(TAG, "Proxy stopped")
    }

    fun getProxy(): ProxyServer? = proxyServer

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "MiTube Proxy",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "Proxy for video streaming"
                setShowBadge(false)
            }
            val manager = getSystemService(NotificationManager::class.java)
            manager.createNotificationChannel(channel)
        }
    }

    private fun buildNotification(): Notification {
        val stopIntent = Intent(this, ProxyForegroundService::class.java).apply {
            action = ACTION_STOP
        }
        val stopPendingIntent = PendingIntent.getService(
            this, 0, stopIntent,
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT
        )

        return NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle("MiTube")
            .setContentText("Proxy activo — reproduciendo")
            .setSmallIcon(android.R.drawable.ic_media_play)
            .setOngoing(true)
            .addAction(android.R.drawable.ic_media_pause, "Detener", stopPendingIntent)
            .build()
    }

    override fun onBind(intent: Intent?): IBinder? = null

    companion object {
        const val ACTION_START = "com.mitube.action.START_PROXY"
        const val ACTION_STOP = "com.mitube.action.STOP_PROXY"
        const val EXTRA_PORT = "proxy_port"
        const val EXTRA_HTML = "player_html"
        private const val CHANNEL_ID = "mitube_proxy"
        private const val NOTIFICATION_ID = 1001
        private const val TAG = "ProxyForegroundService"
    }
}
