package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.GoalsItemBinding;
import com.rpm.clynx.model.GoalsItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link GoalsItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class GoalsAdapter extends RecyclerView.Adapter<GoalsAdapter.GoalsViewHolder> {

    private final List<GoalsItemModel> gValues;
    public Context cxt;

    public GoalsAdapter(List<GoalsItemModel> items) {
        gValues = items;
    }

    public GoalsAdapter(List<GoalsItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public GoalsViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new GoalsViewHolder(GoalsItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final GoalsViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mGoalTime.setText(gValues.get(position).getGoal());
        holder.mGoalText.setText(gValues.get(position).getDescription());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class GoalsViewHolder extends RecyclerView.ViewHolder {
        public final TextView mGoalTime;
        public final TextView mGoalText;
        public GoalsItemModel mItem;

        public GoalsViewHolder(GoalsItemBinding binding) {
            super(binding.getRoot());
            mGoalTime = binding.itemGoalTime;
            mGoalText = binding.itemGoalText;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mGoalText.getText() + "'";
        }
    }
}