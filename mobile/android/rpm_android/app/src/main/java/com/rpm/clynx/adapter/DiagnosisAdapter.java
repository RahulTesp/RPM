package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.DiagnosisItemBinding;
import com.rpm.clynx.model.DiagnosisItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link DiagnosisItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class DiagnosisAdapter extends RecyclerView.Adapter<DiagnosisAdapter.DiagnosisViewHolder> {

    private final List<DiagnosisItemModel> gValues;
    public Context cxt;

    public DiagnosisAdapter(List<DiagnosisItemModel> items) {
        gValues = items;
    }

    public DiagnosisAdapter(List<DiagnosisItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public DiagnosisViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new DiagnosisViewHolder(DiagnosisItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final DiagnosisViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mDiagnosisName.setText(gValues.get(position).getDiagnosisName());
        holder.mDiagnosisCode.setText(gValues.get(position).getDiagnosisCode());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class DiagnosisViewHolder extends RecyclerView.ViewHolder {
        public final TextView mDiagnosisName;
        public final TextView mDiagnosisCode;
        public DiagnosisItemModel mItem;

        public DiagnosisViewHolder(DiagnosisItemBinding binding) {
            super(binding.getRoot());
            mDiagnosisName = binding.itemDiagnosis;
            mDiagnosisCode = binding.itemDiagcode;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mDiagnosisName.getText() + "'";
        }
    }
}