package com.rpm.clynx.adapter;

import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import com.rpm.clynx.R;

import java.time.LocalDate;
import java.time.format.TextStyle;
import java.util.List;
import java.util.Locale;

public class ToDoDateAdapter extends RecyclerView.Adapter<ToDoDateAdapter.DateViewHolder> {

    private List<LocalDate> dates;
    private OnDateClickListener listener;
    private LocalDate selectedDate = LocalDate.now(); // today by default

    public interface OnDateClickListener {
        void onDateClick(LocalDate date);
    }

    public ToDoDateAdapter(List<LocalDate> dates, OnDateClickListener listener) {
        this.dates = dates;
        this.listener = listener;
    }

    @NonNull
    @Override
    public DateViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_todo_date, parent, false);

        //  Calculate width for 7 items per screen
        int screenWidth = parent.getContext().getResources().getDisplayMetrics().widthPixels;

        // RecyclerView padding in pixels (if any)
        int recyclerPadding = (int) (8 * parent.getContext().getResources().getDisplayMetrics().density); // match XML paddingStart/End

        int itemWidth = (screenWidth - recyclerPadding * 2) / 7; // 7 items visible initially
        view.getLayoutParams().width = itemWidth;

        return new DateViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull DateViewHolder holder, int position) {
        LocalDate date = dates.get(position);

        holder.bind(date, listener, date.equals(selectedDate));

        //  Force each item to take 1/7 of screen width
//        int screenWidth = holder.itemView.getResources().getDisplayMetrics().widthPixels;
//        holder.itemView.getLayoutParams().width = screenWidth / 7;
//
//        holder.bind(date, listener, date.equals(selectedDate));
    }

    @Override
    public int getItemCount() {
        return dates.size();
    }

    public void setSelectedDate(LocalDate date) {
        selectedDate = date;
        notifyDataSetChanged();
    }

    static class DateViewHolder extends RecyclerView.ViewHolder {
        TextView tvDay, tvDate;

        DateViewHolder(@NonNull View itemView) {
            super(itemView);
            tvDay = itemView.findViewById(R.id.tv_day);
            tvDate = itemView.findViewById(R.id.tv_date);
        }

        void bind(LocalDate date, OnDateClickListener listener, boolean isSelected) {
            tvDay.setText(date.getDayOfWeek().getDisplayName(TextStyle.SHORT, Locale.getDefault()));
            tvDate.setText(String.valueOf(date.getDayOfMonth()));

            if (isSelected) {
                // Highlight today/selected date
                tvDate.setTextColor(Color.WHITE);
                tvDate.setBackgroundResource(R.drawable.bg_todo_date_circle);
            } else {
                tvDate.setTextColor(ContextCompat.getColor(tvDate.getContext(), R.color.todolist_color1));
                tvDate.setBackground(null);
            }

            itemView.setOnClickListener(v -> listener.onDateClick(date));
        }
    }
}
