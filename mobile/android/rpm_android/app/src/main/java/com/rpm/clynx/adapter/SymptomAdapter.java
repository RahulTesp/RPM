package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.SymptomsItemBinding;
import com.rpm.clynx.model.SymptomItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link SymptomItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class SymptomAdapter extends RecyclerView.Adapter<SymptomAdapter.SymptomViewHolder> {

    private final List<SymptomItemModel> gValues;
    public Context cxt;

    public SymptomAdapter(List<SymptomItemModel> items) {
        gValues = items;
    }

    public SymptomAdapter(List<SymptomItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public SymptomViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new SymptomViewHolder(SymptomsItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final SymptomViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mSymptom.setText(gValues.get(position).getSymptom());
        holder.mSymptomStartDateTime.setText(gValues.get(position).getSymptomStartDateTime());
        holder.mDescription.setText(gValues.get(position).getDescription());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }
    public class SymptomViewHolder extends RecyclerView.ViewHolder {
        public final TextView mSymptom;
        public final TextView mSymptomStartDateTime;
        public final TextView mDescription;
        public SymptomItemModel mItem;

        public SymptomViewHolder(SymptomsItemBinding binding) {
            super(binding.getRoot());
            mSymptom = binding.itemSymp;
            mSymptomStartDateTime = binding.itemDate;
            mDescription = binding.itemDesc;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mSymptom.getText() + "'";
        }
    }
}