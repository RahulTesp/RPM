package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.ChannelItemBinding;
import com.rpm.clynx.fragments.MessageActivity;
import com.rpm.clynx.model.ChannelItemModel;
import com.rpm.clynx.utility.FileLogger;
import android.util.Log;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link ChannelItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class ChannelAdapter extends RecyclerView.Adapter<ChannelAdapter.ChannelViewHolder> {
    private final List<ChannelItemModel> gValues;
    public Context cxt;
    String Token;
    public ChannelAdapter(List<ChannelItemModel> items) {
        gValues = items;
    }

    public ChannelAdapter(List<ChannelItemModel> items,Context cxt, String token) {
        gValues = items;
        this.cxt = cxt;
        this.Token = token;
    }

    @Override
    public ChannelViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new ChannelViewHolder(ChannelItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final ChannelViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.memberUsrnmText.setText(gValues.get(position).getMemberUsrnm());
        holder.memberNmText.setText(gValues.get(position).getMemberName());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }
    public class ChannelViewHolder extends RecyclerView.ViewHolder {
        public final TextView memberUsrnmText;
        public final TextView memberNmText;
        public ChannelItemModel mItem;

        public ChannelViewHolder(ChannelItemBinding binding) {
            super(binding.getRoot());

            memberUsrnmText = binding.itemMemUsrNm;
            memberNmText = binding.itemMemNm;
            memberNmText.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                   //  Open the new activity here
                    Context context = v.getContext();
                    Intent intent = new Intent(context, MessageActivity.class);
                    intent.putExtra("memberName",memberNmText.getText().toString());
                    intent.putExtra("memberUsername",memberNmText.getText().toString());
                    intent.putExtra("FriendlyUsername", memberUsrnmText.getText().toString() + "-" + memberUsrnmText.getText().toString());

                    intent.putExtra("channel","channel");

                    String memberName = memberNmText.getText().toString();
                    String memberUsername = memberUsrnmText.getText().toString();
                    String friendlyUsername = memberUsername + "-" + memberUsername;

                    context.startActivity(intent);
                    // Finish the current activity
                    ((Activity) context).finish();
                }
            });
        }

        @Override
        public String toString() {
            return super.toString() + " '" + memberUsrnmText.getText() + "'";
        }
    }
}