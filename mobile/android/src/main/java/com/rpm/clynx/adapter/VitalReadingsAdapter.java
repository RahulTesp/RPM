package com.rpm.clynx.adapter;

import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.R;
import com.rpm.clynx.databinding.VitalReadingsItemBinding;
import com.rpm.clynx.model.VitalReadingsItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalReadingsItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalReadingsAdapter extends RecyclerView.Adapter<VitalReadingsAdapter.VitalReadingsViewHolder> {

    private final List<VitalReadingsItemModel> gValues;
    public Context cxt;

    public VitalReadingsAdapter(List<VitalReadingsItemModel> items) {
        gValues = items;
    }

    public VitalReadingsAdapter(List<VitalReadingsItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public VitalReadingsViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new VitalReadingsViewHolder(VitalReadingsItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final VitalReadingsViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mReadTime.setText(gValues.get(position).getReadingTime());
        // Check if mDiastolic is not an empty string
        String systolicValue = gValues.get(position).getSystolic();
        String diastolicValue = gValues.get(position).getDiastolic();
        String pulseValue = gValues.get(position).getPulse();
        String oxygenValue = gValues.get(position).getOxygen();
        String pulSEValue = gValues.get(position).getPulSE();
        String weightValue = gValues.get(position).getWeight();
        String glucoseValue = gValues.get(position).getGlucose();
        String scheduleValue = gValues.get(position).getSchedule();
        String systolicstatusValue = gValues.get(position).getSystolicStatus();
        String diastolicstatusvalue = gValues.get(position).getDiastolicStatus();
        String pulsestatusvalue = gValues.get(position).getPulseStatus();
        String statusValue = gValues.get(position).getStatus();
        String oxygenstatusvalue = gValues.get(position).getOxygenStatus();
        String pulseStatuSvalue = gValues.get(position).getPulseStatuS();

        if (statusValue.equals("Critical")) {
            holder.mShape1.setBackgroundResource(R.drawable.custom_shape_critical);
            holder.mShape2.setBackgroundResource(R.drawable.custom_shape_critical);
            holder.mShape1.setVisibility(View.VISIBLE);
            holder.mShape2.setVisibility(View.VISIBLE);
        } else if (statusValue.equals("Cautious")) {
            holder.mShape1.setBackgroundResource(R.drawable.custom_shape_cautious);
            holder.mShape2.setBackgroundResource(R.drawable.custom_shape_cautious);
            holder.mShape1.setVisibility(View.VISIBLE);
            holder.mShape2.setVisibility(View.VISIBLE);
        } else if (statusValue.equals("Normal")) {
            holder.mShape1.setVisibility(View.GONE);
            holder.mShape2.setVisibility(View.GONE);
        }
        else {
            holder.mShape1.setVisibility(View.GONE);
            holder.mShape2.setVisibility(View.GONE);
        }

        // Check if diastolicValue is not empty
        if (!systolicValue.isEmpty()) {
            holder.mSystolic.setText(systolicValue);
            if (systolicstatusValue.equals("Critical")) {
                holder.mSystolic.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (systolicstatusValue.equals("Cautious")) {
                holder.mSystolic.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mSystolic.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mSystolic.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        // Repeat the above logic for other TextViews
        // Check if diastolicValue is not empty
        if (!diastolicValue.isEmpty()) {
            holder.mDiastolic.setText(" / " + diastolicValue + " mmHg");
            if (diastolicstatusvalue.equals("Critical")) {
                holder.mDiastolic.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (diastolicstatusvalue.equals("Cautious")) {
                holder.mDiastolic.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mDiastolic.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mDiastolic.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        // Check if diastolicValue is not empty
        if (!pulseValue.isEmpty()) {
            holder.mPulse.setText(pulseValue + " bpm");
            if (pulsestatusvalue.equals("Critical")) {
                holder.mPulse.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (pulsestatusvalue.equals("Cautious")) {
                holder.mPulse.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mPulse.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mPulse.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        if (!oxygenValue.isEmpty()) {
            holder.mOxygen.setText(oxygenValue + " %");
            if (oxygenstatusvalue.equals("Critical")) {
                holder.mOxygen.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (oxygenstatusvalue.equals("Cautious")) {
                holder.mOxygen.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mOxygen.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mOxygen.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        if (!pulSEValue.isEmpty()) {
            holder.mPulSE.setText(pulSEValue + " bpm");
            if (pulseStatuSvalue.equals("Critical")) {
                holder.mPulSE.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (pulseStatuSvalue.equals("Cautious")) {
                holder.mPulSE.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mPulSE.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mPulSE.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        if (!weightValue.isEmpty()) {
            holder.mWeight.setText(weightValue + " lbs");
            holder.mShape2.setVisibility(View.GONE);
            if (statusValue.equals("Critical")) {
                holder.mWeight.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (statusValue.equals("Cautious")) {
                holder.mWeight.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mWeight.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mWeight.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        if (!glucoseValue.isEmpty()) {
            holder.mGlucose.setText(glucoseValue + " mgdl");
            if (statusValue.equals("Critical")) {
                holder.mGlucose.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (statusValue.equals("Cautious")) {
                holder.mGlucose.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mGlucose.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mGlucose.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }

        if (!scheduleValue.isEmpty()) {
            holder.mSchedule.setText(scheduleValue);
            if (statusValue.equals("Critical")) {
                holder.mSchedule.setTextColor(ContextCompat.getColor(cxt, R.color.red_900));
            } else if (statusValue.equals("Cautious")) {
                holder.mSchedule.setTextColor(ContextCompat.getColor(cxt, R.color.cautious));
            } else {
                holder.mSchedule.setTextColor(ContextCompat.getColor(cxt, R.color.black));
            }
        } else {
            // If diastolicValue is empty, set the text color to your default color (e.g., black)
            holder.mSchedule.setTextColor(ContextCompat.getColor(cxt, R.color.black));
        }
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class VitalReadingsViewHolder extends RecyclerView.ViewHolder {
        public final TextView mReadTime;
        public final TextView mSystolic;
        public final TextView mDiastolic;
        public final TextView mPulse;
        public final TextView mOxygen;
        public final TextView mPulSE;
        public final TextView mWeight;
        public final TextView mGlucose;
        public final TextView mSchedule;
        public VitalReadingsItemModel mItem;
        View mShape1;
        View mShape2;

        public VitalReadingsViewHolder(VitalReadingsItemBinding binding) {
            super(binding.getRoot());
            mReadTime = binding.itemReadTime;
            mSystolic = binding.itemSysto;
            mDiastolic = binding.itemDiastolic;
            mPulse = binding.itemPulse;
            mOxygen = binding.itemOxy;
            mPulSE = binding.itemPulse;
            mWeight = binding.itemWeight;
            mGlucose = binding.itemGlu;
            mSchedule = binding.itemSched;
            mShape1 = binding.itemShape1;
            mShape2 = binding.itemShape2;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mDiastolic.getText() + "/";
        }
    }
}