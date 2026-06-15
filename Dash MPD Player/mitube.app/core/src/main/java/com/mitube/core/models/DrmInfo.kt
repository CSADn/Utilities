package com.mitube.core.models

data class DrmInfo(
    val systemId: String,
    val licenseUrl: String
) {
    val isWidevine: Boolean
        get() = systemId.equals("edef8ba9-79d6-4ace-a3c8-27dcd51d21ed", ignoreCase = true)

    val isPlayReady: Boolean
        get() = systemId.equals("9a04f079-9840-4286-ab92-e65be0885f95", ignoreCase = true)

    val isClearKey: Boolean
        get() = systemId.equals("e2719d58-a985-b3c9-781a-b030af78d30e", ignoreCase = true)

    companion object {
        const val WIDEVINE_UUID = "edef8ba9-79d6-4ace-a3c8-27dcd51d21ed"
        const val PLAYREADY_UUID = "9a04f079-9840-4286-ab92-e65be0885f95"
        const val CLEARKEY_UUID = "e2719d58-a985-b3c9-781a-b030af78d30e"
    }
}
