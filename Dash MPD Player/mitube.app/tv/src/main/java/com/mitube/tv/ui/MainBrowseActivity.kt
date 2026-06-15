package com.mitube.tv.ui

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.mitube.core.api.ApiClient
import com.mitube.core.models.Channel
import com.mitube.tv.MiTubeTvApp
import com.mitube.tv.R
import com.mitube.tv.ui.MainBrowseFragment.Companion.EXTRA_CHANNEL
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class MainBrowseActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main_browse)

        if (MiTubeTvApp.instance.sessionManager.authToken == null) {
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }

        CoroutineScope(Dispatchers.Main).launch {
            val valid = withContext(Dispatchers.IO) {
                try {
                    val response = ApiClient.api.validate()
                    response.isSuccessful
                } catch (_: Exception) { false }
            }
            if (!valid) {
                MiTubeTvApp.instance.sessionManager.clear()
                ApiClient.setToken(null)
                startActivity(Intent(this@MainBrowseActivity, LoginActivity::class.java).apply {
                    flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                })
                finish()
                return@launch
            }

            if (savedInstanceState == null) {
                supportFragmentManager.beginTransaction()
                    .replace(R.id.browseContainer, MainBrowseFragment())
                    .commit()
            }
        }
    }
}
