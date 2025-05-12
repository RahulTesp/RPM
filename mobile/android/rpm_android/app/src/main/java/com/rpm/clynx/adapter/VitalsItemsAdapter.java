package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.VitalItemsModel;
import com.rpm.clynx.R;
import java.util.ArrayList;

public class VitalsItemsAdapter extends RecyclerView.Adapter<VitalsItemsAdapter.ViewHolder> {

    ArrayList<VitalItemsModel> alist;
    private final LayoutInflater inflater;
    public VitalsItemsAdapter(Context context, ArrayList<VitalItemsModel> alist) {
        this.alist =alist;
        inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
    }

    @Override
    public VitalsItemsAdapter.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View rootView = inflater.inflate(R.layout.subitem_item_vitallist, parent, false);
        return new VitalsItemsAdapter.ViewHolder(rootView);
    }

    @Override
    public void onBindViewHolder(VitalsItemsAdapter.ViewHolder holder, int position) {
        VitalItemsModel vitalItemsModel = alist.get(position);
        holder.value.setText(vitalItemsModel.getValue()+ " " +vitalItemsModel.getUnit() );
        holder.time.setText(vitalItemsModel.getTime() );
    }

    @Override
    public int getItemCount() {
        return alist.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public String ID;
        TextView value;
        TextView time;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
          time = itemView.findViewById(R.id.subitemvital_time);
          value = itemView.findViewById(R.id.subitemvital_value);
        }
    }
}
