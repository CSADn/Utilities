package com.mitube.mobile.ui

import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.widget.EditText
import android.widget.ImageView
import android.widget.TextView
import android.widget.ViewSwitcher
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.SearchView
import com.google.gson.Gson
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout
import com.mitube.core.api.ApiClient
import com.mitube.core.models.ChannelGroup
import com.mitube.mobile.MiTubeMobileApp
import com.mitube.core.SessionManager
import com.mitube.mobile.R
import com.mitube.mobile.adapters.CategoryAdapter
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class HomeActivity : AppCompatActivity() {

    private lateinit var categoryRecyclerView: RecyclerView
    private lateinit var channelCountText: TextView
    private lateinit var userNameText: TextView
    private lateinit var logoutBtn: ImageView
    private lateinit var searchView: SearchView
    private lateinit var swipeRefresh: SwipeRefreshLayout
    private lateinit var viewSwitcher: ViewSwitcher
    private var allGroups: List<ChannelGroup> = emptyList()
    private val adapter = CategoryAdapter { channel ->
        val intent = Intent(this, PlayerActivity::class.java).apply {
            putExtra("channel_name", channel.name)
            putExtra("channel_url", channel.url)
            putExtra("channel_type", channel.type)
            putExtra("channel_drm_license_uri", channel.drm_license_uri)
            putExtra("channel_icono", channel.icono)
            putExtra("channel_headers", Gson().toJson(channel.headers))
        }
        startActivity(intent)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_home)

        categoryRecyclerView = findViewById(R.id.categoryRecyclerView)
        channelCountText = findViewById(R.id.channelCountText)
        userNameText = findViewById(R.id.userNameText)
        logoutBtn = findViewById(R.id.logoutBtn)
        searchView = findViewById(R.id.searchView)

        val searchEditText = searchView.findViewById<EditText>(androidx.appcompat.R.id.search_src_text)
        searchEditText?.setTextColor(Color.WHITE)
        searchEditText?.setHintTextColor(Color.parseColor("#aaaaaa"))
        searchView.findViewById<ImageView>(androidx.appcompat.R.id.search_mag_icon)
            ?.setColorFilter(Color.WHITE)
        searchView.findViewById<ImageView>(androidx.appcompat.R.id.search_close_btn)
            ?.setColorFilter(Color.WHITE)
        swipeRefresh = findViewById(R.id.swipeRefresh)
        viewSwitcher = findViewById(R.id.viewSwitcher)

        categoryRecyclerView.layoutManager = LinearLayoutManager(this)
        categoryRecyclerView.adapter = adapter

        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?) = false
            override fun onQueryTextChange(newText: String?): Boolean {
                filterChannels(newText ?: "")
                return true
            }
        })

        swipeRefresh.setOnRefreshListener { loadChannels() }

        val session = MiTubeMobileApp.instance.sessionManager
        userNameText.text = session.displayName ?: session.lastUsername ?: ""

        logoutBtn.setOnClickListener {
            session.clear()
            ApiClient.setToken(null)
            startActivity(Intent(this, LoginActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            })
            finish()
        }

        if (session.authToken == null) {
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }

        CoroutineScope(Dispatchers.Main).launch {
            val valid = withContext(Dispatchers.IO) {
                try {
                    val response = ApiClient.api.validate()
                    response.isSuccessful
                } catch (_: Exception) { false }
            }
            if (!valid) {
                MiTubeMobileApp.instance.sessionManager.clear()
                ApiClient.setToken(null)
                startActivity(Intent(this@HomeActivity, LoginActivity::class.java).apply {
                    flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                })
                finish()
                return@launch
            }
            loadChannels()
        }
    }

    private fun loadChannels() {
        swipeRefresh.isRefreshing = true
        CoroutineScope(Dispatchers.Main).launch {
            val result = withContext(Dispatchers.IO) {
                try {
                    val response = ApiClient.api.getChannels()
                    if (response.isSuccessful) response.body() ?: emptyList()
                    else {
                        if (response.code() == 401) {
                            MiTubeMobileApp.instance.sessionManager.clear()
                            ApiClient.setToken(null)
                            startActivity(Intent(this@HomeActivity, LoginActivity::class.java).apply {
                                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                            })
                            finish()
                        }
                        emptyList()
                    }
                } catch (_: Exception) { emptyList() }
            }
            allGroups = result
            adapter.submitList(result)
            updateCount(result)
            restoreLastChannel(result)
            swipeRefresh.isRefreshing = false
            viewSwitcher.displayedChild = if (result.isEmpty()) 0 else 1
        }
    }

    private fun filterChannels(query: String) {
        if (query.isBlank()) {
            adapter.submitList(allGroups)
            updateCount(allGroups)
            return
        }

        val filtered = allGroups.mapNotNull { group ->
            val matched = group.samples.filter {
                it.name.contains(query, ignoreCase = true)
            }
            if (matched.isEmpty()) null
            else group.copy(samples = matched)
        }
        adapter.submitList(filtered)
        updateCount(filtered)
    }

    private fun updateCount(groups: List<ChannelGroup>) {
        val total = groups.sumOf { it.samples.size }
        channelCountText.text = "$total canales"
    }

    private fun restoreLastChannel(groups: List<ChannelGroup>) {
        val lastChannel = MiTubeMobileApp.instance.sessionManager.lastChannelName ?: return
        val groupIndex = groups.indexOfFirst { group ->
            group.samples.any { it.name == lastChannel }
        }
        if (groupIndex >= 0) {
            categoryRecyclerView.post {
                categoryRecyclerView.smoothScrollToPosition(groupIndex)
            }
        }
    }
}
