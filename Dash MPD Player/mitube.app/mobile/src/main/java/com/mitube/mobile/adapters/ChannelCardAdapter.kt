package com.mitube.mobile.adapters

import android.graphics.Color
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import coil.load
import com.google.android.material.card.MaterialCardView
import com.mitube.core.models.Channel

class ChannelCardAdapter(
    private val onChannelClick: (Channel) -> Unit
) : RecyclerView.Adapter<ChannelCardAdapter.ChannelViewHolder>() {

    private var channels: List<Channel> = emptyList()

    fun submitList(newChannels: List<Channel>) {
        channels = newChannels
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ChannelViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(com.mitube.mobile.R.layout.item_channel_card, parent, false)
        return ChannelViewHolder(view, onChannelClick)
    }

    override fun onBindViewHolder(holder: ChannelViewHolder, position: Int) {
        holder.bind(channels[position])
        val id = holder.itemView.id
        if (position == 0) {
            holder.itemView.nextFocusLeftId = id
        } else {
            holder.itemView.nextFocusLeftId = View.NO_ID
        }
        if (position == channels.size - 1) {
            holder.itemView.nextFocusRightId = id
        } else {
            holder.itemView.nextFocusRightId = View.NO_ID
        }
    }

    override fun getItemCount() = channels.size

    class ChannelViewHolder(
        itemView: View,
        onChannelClick: (Channel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {
        private val channelIcon: ImageView = itemView.findViewById(com.mitube.mobile.R.id.channelIcon)
        private val channelName: TextView = itemView.findViewById(com.mitube.mobile.R.id.channelName)
        private var currentChannel: Channel? = null

        init {
            if (itemView.id == View.NO_ID) {
                itemView.id = View.generateViewId()
            }
            itemView.setOnClickListener {
                currentChannel?.let(onChannelClick)
            }
            itemView.onFocusChangeListener = View.OnFocusChangeListener { v, hasFocus ->
                val card = v as MaterialCardView
                if (hasFocus) {
                    card.strokeWidth = 2
                    card.setStrokeColor(Color.WHITE)
                    card.setCardBackgroundColor(Color.parseColor("#3d3d3d"))
                } else {
                    card.strokeWidth = 0
                    card.setCardBackgroundColor(Color.parseColor("#2a2a2a"))
                }
            }
        }

        fun bind(channel: Channel) {
            currentChannel = channel
            channelName.text = channel.name
            if (channel.icono.isNotBlank()) {
                channelIcon.load(channel.icono) {
                    crossfade(true)
                    placeholder(com.mitube.mobile.R.drawable.ic_channel_placeholder)
                    error(android.R.drawable.ic_menu_help)
                }
            } else {
                channelIcon.setImageResource(android.R.drawable.ic_menu_help)
            }
        }
    }
}
