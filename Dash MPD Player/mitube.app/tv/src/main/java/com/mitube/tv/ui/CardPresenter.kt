package com.mitube.tv.ui

import android.view.ViewGroup
import androidx.leanback.widget.ImageCardView
import androidx.leanback.widget.Presenter
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
import com.mitube.core.models.Channel

class CardPresenter : Presenter() {

    override fun onCreateViewHolder(parent: ViewGroup): Presenter.ViewHolder {
        val cardView = ImageCardView(parent.context).apply {
            isFocusable = true
            isFocusableInTouchMode = true
            cardType = ImageCardView.CARD_TYPE_INFO_UNDER
        }
        return ViewHolder(cardView)
    }

    override fun onBindViewHolder(viewHolder: Presenter.ViewHolder, item: Any) {
        val channel = item as Channel
        val cardView = viewHolder.view as ImageCardView

        cardView.titleText = channel.name
        cardView.contentText = channel.type

        if (channel.icono.isNotBlank()) {
            Glide.with(cardView.context)
                .load(channel.icono)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .centerCrop()
                .error(android.R.drawable.ic_menu_help)
                .into(cardView.mainImageView)
        } else {
            cardView.setMainImage(cardView.context.getDrawable(android.R.drawable.ic_menu_help)!!)
        }
    }

    override fun onUnbindViewHolder(viewHolder: Presenter.ViewHolder) {}

    private class ViewHolder(view: ImageCardView) : Presenter.ViewHolder(view)
}
