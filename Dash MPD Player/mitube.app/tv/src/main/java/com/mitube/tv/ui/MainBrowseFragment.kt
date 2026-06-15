package com.mitube.tv.ui

import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.leanback.app.BrowseSupportFragment
import androidx.leanback.widget.ArrayObjectAdapter
import androidx.leanback.widget.HeaderItem
import androidx.leanback.widget.ListRow
import androidx.leanback.widget.ListRowPresenter
import androidx.leanback.widget.OnItemViewClickedListener
import androidx.leanback.widget.Presenter
import androidx.leanback.widget.Row
import androidx.leanback.widget.RowPresenter
import com.mitube.core.api.ApiClient
import com.mitube.core.models.Channel
import com.mitube.core.models.ChannelGroup
import com.mitube.tv.MiTubeTvApp
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class MainBrowseFragment : BrowseSupportFragment() {

    private val rowsAdapter = ArrayObjectAdapter(ListRowPresenter())

    override fun onActivityCreated(savedInstanceState: Bundle?) {
        super.onActivityCreated(savedInstanceState)
        title = "MiTube TV"
        adapter = rowsAdapter

        setOnItemViewClickedListener(ItemViewClickedListener())

        loadChannels()
    }

    private fun loadChannels() {
        headersState = HEADERS_ENABLED
        CoroutineScope(Dispatchers.Main).launch {
            val groups = withContext(Dispatchers.IO) {
                try {
                    val response = ApiClient.api.getChannels()
                    if (response.isSuccessful) response.body() ?: emptyList()
                    else {
                        if (response.code() == 401) {
                            MiTubeTvApp.instance.sessionManager.clear()
                            ApiClient.setToken(null)
                            activity?.let {
                                it.startActivity(Intent(it, LoginActivity::class.java).apply {
                                    flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                                })
                                it.finish()
                            }
                        }
                        emptyList()
                    }
                } catch (_: Exception) { emptyList() }
            }
            if (groups.isEmpty()) {
                Toast.makeText(activity, "No se pudieron cargar los canales", Toast.LENGTH_LONG).show()
            }
            populateRows(groups)
        }
    }

    private fun populateRows(groups: List<ChannelGroup>) {
        rowsAdapter.clear()
        groups.forEach { group ->
            val cardAdapter = ArrayObjectAdapter(CardPresenter())
            group.samples.forEach { channel -> cardAdapter.add(channel) }
            val header = HeaderItem(group.name)
            rowsAdapter.add(ListRow(header, cardAdapter))
        }
    }

    private inner class ItemViewClickedListener : OnItemViewClickedListener {
        override fun onItemClicked(
            itemViewHolder: Presenter.ViewHolder?,
            item: Any?,
            rowViewHolder: RowPresenter.ViewHolder?,
            row: Row?
        ) {
            if (item is Channel) {
                val intent = Intent(activity, PlayerActivity::class.java).apply {
                    putExtra("channel_name", item.name)
                    putExtra("channel_url", item.url)
                    putExtra("channel_type", item.type)
                    putExtra("channel_drm_license_uri", item.drm_license_uri)
                    putExtra("channel_icono", item.icono)
                }
                startActivity(intent)
            }
        }
    }

    companion object {
        const val EXTRA_CHANNEL = "channel"
        private const val TAG = "MainBrowseFragment"
    }
}
