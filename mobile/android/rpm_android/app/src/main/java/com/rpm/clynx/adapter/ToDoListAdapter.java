package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.ToDoListModel;
import java.util.List;
import com.rpm.clynx.R;

public class ToDoListAdapter extends RecyclerView.Adapter<ToDoListAdapter.ViewHolder> {
    private final List<ToDoListModel> toDoListModels;
    private final LayoutInflater inflater;
    public ToDoListAdapter(Context context, List<ToDoListModel> todoModel) {
        this.toDoListModels = todoModel;
        inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
    }

    @Override
    public ToDoListAdapter.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View rootView = inflater.inflate(R.layout.item_todo_list, parent, false);
        return new ToDoListAdapter.ViewHolder(rootView);
    }

    @Override
    public void onBindViewHolder(ToDoListAdapter.ViewHolder holder, int position) {
        holder.type.setText(toDoListModels.get(position).getScheduleType());
        holder.time.setText(toDoListModels.get(position).getTime());
        holder.description.setText(toDoListModels.get(position).getDecription());
    }

    @Override
    public int getItemCount() {
        return toDoListModels.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public String ID;
        TextView type;
        TextView time;
        TextView description;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            type = itemView.findViewById(R.id.item_todo_list_type);
            time = itemView.findViewById(R.id.item_todo_list_time);
            description = itemView.findViewById(R.id.item_todo_list_type_name);
        }
    }
}
