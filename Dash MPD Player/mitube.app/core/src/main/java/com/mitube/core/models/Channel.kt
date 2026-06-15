package com.mitube.core.models

import com.google.gson.annotations.SerializedName

data class Channel(
    val name: String = "",
    val url: String = "",
    val type: String = "",
    val drm_license_uri: String = "",
    val icono: String = "",
    val headers: Map<String, String>? = null,
    val manifestUrl: String = "",
    val licenseProxyUrl: String = "",
    val keyId: String = "",
    val key: String = ""
) {
    data class ClearkeyLicense(val keyId: String, val key: String)

    fun parseClearkeyLicense(): ClearkeyLicense? {
        if (drm_license_uri.isBlank()) return null
        val uri = java.net.URI(drm_license_uri)
        val params = uri.query?.split("&")
            ?.mapNotNull { param ->
                val parts = param.split("=", limit = 2)
                if (parts.size == 2) parts[0] to parts[1]
                else null
            }?.toMap() ?: return null

        val keyId = params["keyid"] ?: return null
        val key = params["key"] ?: return null
        if (keyId.isBlank() || key.isBlank()) return null
        return ClearkeyLicense(keyId, key)
    }

    val isClearkey: Boolean get() = type.equals("CLEARKEY", ignoreCase = true)
    val isWidevine: Boolean get() = type.equals("WIDEVINE", ignoreCase = true)
    val isHls: Boolean get() = type.equals("HLS", ignoreCase = true)
    val isFlow: Boolean get() = headers != null
}
