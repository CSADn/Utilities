package com.mitube.core.api

import com.mitube.core.models.ChannelGroup
import com.mitube.core.models.LoginRequest
import com.mitube.core.models.LoginResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

interface MitubeApi {
    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>

    @GET("api/auth/validate")
    suspend fun validate(): Response<Map<String, Any>>

    @GET("api/channels")
    suspend fun getChannels(): Response<List<ChannelGroup>>
}
