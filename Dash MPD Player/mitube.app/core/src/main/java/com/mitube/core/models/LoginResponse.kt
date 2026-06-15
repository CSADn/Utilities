package com.mitube.core.models

data class LoginRequest(
    val username: String,
    val password: String
)

data class LoginResponse(
    val token: String = "",
    val expiresIn: Int = 0,
    val username: String = "",
    val displayName: String = ""
)
