package com.mitube.tv

import android.app.Application
import com.mitube.core.SessionManager
import com.mitube.core.api.ApiClient

class MiTubeTvApp : Application() {
    lateinit var sessionManager: SessionManager
        private set

    override fun onCreate() {
        super.onCreate()
        instance = this
        sessionManager = SessionManager(this)
        sessionManager.serverUrl?.let { ApiClient.configure(it) }
        sessionManager.authToken?.let { ApiClient.restoreToken(it) }
    }

    companion object {
        lateinit var instance: MiTubeTvApp
            private set
    }
}
