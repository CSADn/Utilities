package com.mitube.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.LinearSnapHelper
import androidx.recyclerview.widget.RecyclerView
import androidx.recyclerview.widget.SnapHelper
import com.mitube.core.models.Channel
import com.mitube.core.models.ChannelGroup

class CategoryAdapter(
    private val onChannelClick: (Channel) -> Unit
) : RecyclerView.Adapter<CategoryAdapter.CategoryViewHolder>() {

    private var groups: List<ChannelGroup> = emptyList()
    private var lastFocusedRecyclerView: RecyclerView? = null

    fun submitList(newGroups: List<ChannelGroup>) {
        groups = newGroups
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CategoryViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(com.mitube.mobile.R.layout.item_category, parent, false)
        return CategoryViewHolder(view, onChannelClick, this::onFocusEntered)
    }

    override fun onBindViewHolder(holder: CategoryViewHolder, position: Int) {
        holder.bind(groups[position])
    }

    override fun getItemCount() = groups.size

    private fun onFocusEntered(rv: RecyclerView) {
        lastFocusedRecyclerView?.let { old -> old.post { old.smoothScrollToPosition(0) } }
        lastFocusedRecyclerView = rv
        rv.post {
            rv.smoothScrollToPosition(0)
            rv.post {
                rv.findViewHolderForAdapterPosition(0)?.itemView?.requestFocus()
            }
        }
    }

    class CategoryViewHolder(
        itemView: View,
        onChannelClick: (Channel) -> Unit,
        private val onFocusEntered: (RecyclerView) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {
        private val categoryName: TextView = itemView.findViewById(com.mitube.mobile.R.id.categoryName)
        private val channelRecyclerView: RecyclerView = itemView.findViewById(com.mitube.mobile.R.id.channelRecyclerView)
        private val adapter = ChannelCardAdapter(onChannelClick)

        init {
            channelRecyclerView.layoutManager = LinearLayoutManager(
                itemView.context, LinearLayoutManager.HORIZONTAL, false
            )
            channelRecyclerView.adapter = adapter
            val snapHelper: SnapHelper = LinearSnapHelper()
            snapHelper.attachToRecyclerView(channelRecyclerView)
            channelRecyclerView.isFocusable = true
            itemView.viewTreeObserver.addOnGlobalFocusChangeListener { oldFocus, newFocus ->
                if (newFocus != null && isInside(newFocus, channelRecyclerView)) {
                    val wasInside = oldFocus != null && isInside(oldFocus, channelRecyclerView)
                    if (!wasInside) {
                        onFocusEntered(channelRecyclerView)
                    }
                }
            }
        }

        private fun isInside(view: View, parent: ViewGroup): Boolean {
            var v: View? = view
            while (v != null) {
                if (v == parent) return true
                v = v.parent as? View
            }
            return false
        }

        fun bind(group: ChannelGroup) {
            categoryName.text = group.name
            adapter.submitList(group.samples)
        }
    }
}