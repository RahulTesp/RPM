package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.ChannelModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link ChannelModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class ChannelListAdapter extends RecyclerView.Adapter<ChannelListAdapter.ViewHolder> {

    private final List<ChannelModel> mValues;
    public Context cxt;

    public ChannelListAdapter(List<ChannelModel> items) {
        mValues = items;
    }
    public ChannelListAdapter(List<ChannelModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.channel_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        ChannelAdapter childRecyclerViewAdapter = new ChannelAdapter(mValues.get(position).getChannelList(),holder.crvNotifications.getContext(),mValues.get(position).getToken());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {

        public final RecyclerView crvNotifications;
        public ChannelModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            crvNotifications = itemView.findViewById(R.id.listChannel);
        }
    }
}