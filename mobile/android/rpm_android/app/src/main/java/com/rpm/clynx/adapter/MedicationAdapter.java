package com.rpm.clynx.adapter;

import androidx.core.content.ContextCompat;
import androidx.core.graphics.drawable.DrawableCompat;
import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.graphics.drawable.Drawable;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.rpm.clynx.R;
import com.rpm.clynx.databinding.MedicationItemBinding;
import com.rpm.clynx.model.MedicationItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link MedicationItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class MedicationAdapter extends RecyclerView.Adapter<MedicationAdapter.MedicationViewHolder> {

    private final List<MedicationItemModel> gValues;
    public Context cxt;

    public MedicationAdapter(List<MedicationItemModel> items) {
        gValues = items;
    }

    public MedicationAdapter(List<MedicationItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public MedicationViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new MedicationViewHolder(MedicationItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final MedicationViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mMedicinename.setText(gValues.get(position).getMedicinename());
        holder.mMedicineSchedule.setText(gValues.get(position).getMedicineSchedule());
        holder.mDate.setText(gValues.get(position).getStartDate() + " - " +gValues.get(position).getEndDate());

        // Set background resources based on the values of Morning, AfterNoon, Evening, Night
        setCardBackground(holder.mcardMng, Boolean.parseBoolean(String.valueOf(gValues.get(position).getMorning())));
        setCardBackground(holder.mcardAftr,Boolean.parseBoolean(String.valueOf(gValues.get(position).getAfterNoon())));
        setCardBackground(holder.mcardEve,Boolean.parseBoolean(String.valueOf(gValues.get(position).getEvening())));
        setCardBackground(holder.mcardNgt, Boolean.parseBoolean(String.valueOf(gValues.get(position).getNight())));

        if (holder.mItem.isExpired()) {
            holder.cardContainer.setBackgroundResource(R.drawable.rounded_expired_bg);
        } else {
            holder.cardContainer.setBackgroundResource(R.drawable.rounded_white_bg);
        }
    }

    // Helper method to set card background based on a condition
    private void setCardBackground(LinearLayout card, boolean condition) {
        if (condition) {
            card.setBackgroundResource(R.drawable.medication_card_t_radius);
        } else {
            card.setBackgroundResource(R.drawable.medication_card_f_radius);
        }
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class MedicationViewHolder extends RecyclerView.ViewHolder {
        public final TextView mMedicinename;
        public final TextView mMedicineSchedule;
        public final TextView mMorning;
        public final TextView mAfterNoon;
        public final TextView mEvening;
        public final TextView mNight;
        public final TextView mDate;
        public MedicationItemModel mItem;
        public LinearLayout mcardMng,mcardAftr,mcardEve,mcardNgt;
        public final LinearLayout cardContainer;
        public MedicationViewHolder(MedicationItemBinding binding) {
            super(binding.getRoot());
            mMedicinename = binding.itemMedname;
            mMedicineSchedule = binding.itemMedsched;
            mMorning = binding.itemMedMorning;
            mcardMng = binding.cardMng;
            mcardAftr = binding.cardAftr;
            mcardEve = binding.cardEve;
            mcardNgt = binding.cardNgt;
            mAfterNoon = binding.itemAfternoon;
            mEvening = binding.itemEvening;
            mNight = binding.itemNight;
            mDate = binding.itemDate;
            cardContainer = binding.cardContainer;
            Log.d("response mMorning",mMorning.toString());
            Log.d("getText mMorning", mMorning.getText().toString());
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mMedicinename.getText() + "'";
        }
    }
}