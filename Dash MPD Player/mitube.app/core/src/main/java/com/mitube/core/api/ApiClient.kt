package com.mitube.core.api

import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {

    private var baseUrl: String = "http://localhost:5000/"
    private var authToken: String? = null

    fun getBaseUrl(): String = baseUrl
    fun getAuthToken(): String? = authToken

    fun configure(url: String) {
        baseUrl = url.trimEnd('/') + "/"
    }

    fun setToken(token: String?) {
        authToken = token
    }

    fun restoreToken(token: String?) {
        authToken = token
    }

    val api: MitubeApi by lazy {
        val logging = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }

        val authInterceptor = Interceptor { chain ->
            val request = chain.request().newBuilder()
            authToken?.let {
                request.addHeader("Authorization", "Bearer $it")
            }
            chain.proceed(request.build())
        }

        val client = OkHttpClient.Builder()
            .addInterceptor(authInterceptor)
            .addInterceptor(logging)
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()

        Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(MitubeApi::class.java)
    }
}
