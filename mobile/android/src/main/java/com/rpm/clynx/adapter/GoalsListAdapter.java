package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.GoalsModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link GoalsModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class GoalsListAdapter extends RecyclerView.Adapter<GoalsListAdapter.ViewHolder> {

    private final List<GoalsModel> mValues;
    public Context cxt;

    public GoalsListAdapter(List<GoalsModel> items) {
        mValues = items;
    }
    public GoalsListAdapter(List<GoalsModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.goals_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        GoalsAdapter childRecyclerViewAdapter = new GoalsAdapter(mValues.get(position).getGoalsList(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {

        public final RecyclerView crvNotifications;
        public GoalsModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            crvNotifications = itemView.findViewById(R.id.listGoals);
        }
    }
}