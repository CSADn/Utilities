package com.mitube.mobile

import android.app.Application
import coil.Coil
import coil.ImageLoader
import coil.disk.DiskCache
import coil.memory.MemoryCache
import com.mitube.core.SessionManager
import com.mitube.core.api.ApiClient

class MiTubeMobileApp : Application() {
    lateinit var sessionManager: SessionManager
        private set

    override fun onCreate() {
        super.onCreate()
        instance = this
        sessionManager = SessionManager(this)
        sessionManager.serverUrl?.let { ApiClient.configure(it) }
        sessionManager.authToken?.let { ApiClient.restoreToken(it) }
        Coil.setImageLoader(
            ImageLoader.Builder(this)
                .memoryCache {
                    MemoryCache.Builder(this)
                        .maxSizePercent(0.25)
                        .build()
                }
                .diskCache {
                    DiskCache.Builder()
                        .directory(cacheDir.resolve("image_cache"))
                        .maxSizeBytes(50 * 1024 * 1024)
                        .build()
                }
                .build()
        )
    }

    companion object {
        lateinit var instance: MiTubeMobileApp
            private set
    }
}