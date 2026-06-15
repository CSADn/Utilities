package com.mitube.tv.ui

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.mitube.core.SessionManager
import com.mitube.core.api.ApiClient
import com.mitube.core.models.LoginRequest
import com.mitube.tv.MiTubeTvApp
import com.mitube.tv.R
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class LoginActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val session = MiTubeTvApp.instance.sessionManager
        if (session.authToken != null) {
            startActivity(Intent(this, MainBrowseActivity::class.java))
            finish()
            return
        }

        setContentView(R.layout.activity_login)

        val serverUrl = findViewById<EditText>(R.id.serverUrl)
        val usernameInput = findViewById<EditText>(R.id.username)
        val passwordInput = findViewById<EditText>(R.id.password)
        val loginBtn = findViewById<Button>(R.id.loginBtn)

        session.serverUrl?.let { serverUrl.setText(it) }
        session.lastUsername?.let { usernameInput.setText(it) }

        serverUrl.nextFocusForwardId = R.id.username
        serverUrl.nextFocusDownId = R.id.username
        usernameInput.nextFocusForwardId = R.id.password
        usernameInput.nextFocusDownId = R.id.password
        passwordInput.nextFocusForwardId = R.id.loginBtn
        passwordInput.nextFocusDownId = R.id.loginBtn

        loginBtn.setOnClickListener {
            val baseUrl = serverUrl.text.toString().trim()
            val username = usernameInput.text.toString().trim()
            val password = passwordInput.text.toString().trim()

            if (baseUrl.isBlank() || username.isBlank() || password.isBlank()) {
                Toast.makeText(this, "Completa todos los campos", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            ApiClient.configure(baseUrl)
            loginBtn.isEnabled = false
            loginBtn.text = "Conectando..."

            CoroutineScope(Dispatchers.Main).launch {
                val result = withContext(Dispatchers.IO) {
                    try {
                        val response = ApiClient.api.login(LoginRequest(username, password))
                        if (response.isSuccessful) {
                            val body = response.body()
                            ApiClient.setToken(body?.token)
                            Result.success(body)
                        } else {
                            Result.failure(Exception("Error ${response.code()}: ${response.message()}"))
                        }
                    } catch (e: Exception) {
                        Result.failure(e)
                    }
                }

                loginBtn.isEnabled = true
                loginBtn.text = "Ingresar"

                result.onSuccess { body ->
                    session.authToken = body?.token
                    session.serverUrl = baseUrl
                    session.lastUsername = username
                    session.displayName = body?.displayName
                    Toast.makeText(this@LoginActivity, "Bienvenido ${body?.displayName}", Toast.LENGTH_SHORT).show()
                    startActivity(Intent(this@LoginActivity, MainBrowseActivity::class.java))
                    finish()
                }.onFailure { e ->
                    Toast.makeText(this@LoginActivity, "Error: ${e.message}", Toast.LENGTH_LONG).show()
                }
            }
        }
    }
}
