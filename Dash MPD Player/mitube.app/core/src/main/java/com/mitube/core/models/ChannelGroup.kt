package com.mitube.core.models

data class ChannelGroup(
    val name: String = "",
    val samples: List<Channel> = emptyList()
)
