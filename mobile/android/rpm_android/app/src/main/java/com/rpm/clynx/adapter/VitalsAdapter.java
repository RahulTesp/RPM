package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.VitalItemsModel;
import com.rpm.clynx.model.VitalsModel;
import com.rpm.clynx.R;
import java.util.ArrayList;
import java.util.List;

public class VitalsAdapter extends RecyclerView.Adapter<VitalsAdapter.ViewHolder> {
    private final List<VitalsModel>  vitalsModels;
    List<VitalItemsModel>  vitalItemsModels ;
    ArrayList<VitalItemsModel> aList;
    private RecyclerView.RecycledViewPool viewPool = new RecyclerView.RecycledViewPool();
    private final LayoutInflater inflater;

    public VitalsAdapter(Context context, List<VitalsModel> vitalsModels, ArrayList<VitalItemsModel> aList) {
        this.vitalsModels = vitalsModels;
        this.aList=aList;
        inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
    }

    @Override
    public VitalsAdapter.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View rootView = inflater.inflate(R.layout.item_vitallist, parent, false);
        return new VitalsAdapter.ViewHolder(rootView);
    }

    @Override
    public void onBindViewHolder(VitalsAdapter.ViewHolder holder, int position) {
       VitalsItemsAdapter vitalsItemsAdapter = new VitalsItemsAdapter(holder.recyclerView_vitem_item.getContext(),aList);
       LinearLayoutManager linearLayoutManager = new LinearLayoutManager(holder.recyclerView_vitem_item.getContext());
       holder.recyclerView_vitem_item.setLayoutManager(linearLayoutManager);
       holder.recyclerView_vitem_item.setAdapter(vitalsItemsAdapter);
       holder.heading.setText(vitalsModels.get(position).getVitalName());
    }

    @Override
    public int getItemCount() {
        return vitalsModels.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public String ID;
        TextView heading;
        RecyclerView recyclerView_vitem_item;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            heading = itemView.findViewById(R.id.item_vitel_list_head_name);
            recyclerView_vitem_item = itemView.findViewById(R.id.recyclerview_vitals2);
        }
    }
}
