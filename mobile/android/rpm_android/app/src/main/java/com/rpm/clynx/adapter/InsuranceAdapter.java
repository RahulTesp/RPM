package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.InsuranceItemBinding;
import com.rpm.clynx.model.InsuranceItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link InsuranceItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class InsuranceAdapter extends RecyclerView.Adapter<InsuranceAdapter.InsuranceViewHolder> {

    private final List<InsuranceItemModel> gValues;
    public Context cxt;

    public InsuranceAdapter(List<InsuranceItemModel> items) {
        gValues = items;
    }

    public InsuranceAdapter(List<InsuranceItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public InsuranceViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new InsuranceViewHolder(InsuranceItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final InsuranceViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mVendorName.setText(gValues.get(position).getInsuranceVendorName());

        if(gValues.get(position).getIsPrimary() == "true")
        {
            holder.mIsPrimary.setText("Primary Insurance");
        }
        else
        {
            holder.mIsPrimary.setText("Secondary Insurance(Optional)");
        }
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class InsuranceViewHolder extends RecyclerView.ViewHolder {
        public final TextView mVendorName;
        public final TextView mIsPrimary;
        public InsuranceItemModel mItem;

        public InsuranceViewHolder(InsuranceItemBinding binding) {
            super(binding.getRoot());
            mVendorName = binding.itemInsurance;
            mIsPrimary = binding.itemPrimSec;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mVendorName.getText() + "'";
        }
    }
}