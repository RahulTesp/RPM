package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.DeviceItemBinding;
import com.rpm.clynx.model.DeviceItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link DeviceItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class DeviceAdapter extends RecyclerView.Adapter<DeviceAdapter.DeviceViewHolder> {

    private final List<DeviceItemModel> gValues;
    public Context cxt;

    public DeviceAdapter(List<DeviceItemModel> items) {
        gValues = items;
    }

    public DeviceAdapter(List<DeviceItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public DeviceViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new DeviceViewHolder(DeviceItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final DeviceViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mIval.setText(gValues.get(position).getIval());
        holder.mVitalName.setText(gValues.get(position).getVitalName());
        holder.mDevNo.setText(gValues.get(position).getDeviceNumber());
        holder.mDevComm.setText(gValues.get(position).getDeviceCommunicationType());
        holder.mDevName.setText(gValues.get(position).getDeviceName());
        holder.mDevStatus.setText(gValues.get(position).getDeviceStatus());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class DeviceViewHolder extends RecyclerView.ViewHolder {
        public final TextView mIval;
        public final TextView mVitalName;
        public final TextView mDevNo;
        public final TextView mDevComm;
        public final TextView mDevName;
        public final TextView mDevStatus;
        public DeviceItemModel mItem;

        public DeviceViewHolder(DeviceItemBinding binding) {
            super(binding.getRoot());
            mIval = binding.itemDevIval;
            mVitalName = binding.itemDeviceVitalName;
            mDevNo = binding.itemDeviceNo;
            mDevComm = binding.itemDeviceComm;
            mDevName = binding.itemDeviceName;
            mDevStatus = binding.itemDeviceStatus;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mVitalName.getText() + "'";
        }
    }
}