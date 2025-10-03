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

    private int selectedPosition = RecyclerView.NO_POSITION;

    public ChannelAdapter(List<ChannelItemModel> items, Context cxt, String token) {
        gValues = items;
        this.cxt = cxt;
        this.Token = token;
    }

    @Override
    public ChannelViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new ChannelViewHolder(ChannelItemBinding.inflate(
                LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final ChannelViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.memberUsrnmText.setText(gValues.get(position).getMemberUsrnm());
        holder.memberNmText.setText(gValues.get(position).getMemberName());

        // Make the root layout reflect selection
        holder.itemView.setSelected(selectedPosition == position);

        // Handle click on the whole row
        holder.itemView.setOnClickListener(v -> {
            int previousSelected = selectedPosition;
            selectedPosition = holder.getAdapterPosition();

            notifyItemChanged(previousSelected);
            notifyItemChanged(selectedPosition);

            Context context = v.getContext();
            Intent intent = new Intent(context, MessageActivity.class);
            intent.putExtra("memberName", holder.memberNmText.getText().toString());
            intent.putExtra("memberUsername", holder.memberUsrnmText.getText().toString());
            intent.putExtra("memberFullName", holder.memberNmText.getText().toString());
            intent.putExtra("FriendlyUsername",
                    holder.memberUsrnmText.getText().toString() + "-" + holder.memberUsrnmText.getText().toString());
            intent.putExtra("channel", "channel");

            context.startActivity(intent);
            ((Activity) context).finish();
        });

        // Handle long click on the whole row
        holder.itemView.setOnLongClickListener(v -> {
            int previousSelected = selectedPosition;
            selectedPosition = holder.getAdapterPosition();

            notifyItemChanged(previousSelected);
            notifyItemChanged(selectedPosition);

            // Optional: handle extra action here
            return true;
        });
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
        }

        @Override
        public String toString() {
            return super.toString() + " '" + memberUsrnmText.getText() + "'";
        }
    }
}