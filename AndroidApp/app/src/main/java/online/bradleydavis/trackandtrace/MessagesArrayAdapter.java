package online.bradleydavis.trackandtrace;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import java.util.ArrayList;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

class MessagesArrayAdapter extends ArrayAdapter {

    public MessagesArrayAdapter(@NonNull Context context, ArrayList<SingleMessage> list) {
        super(context, 0, list);
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        View formattedMessage = LayoutInflater.from(super.getContext()).inflate(R.layout.single_message, parent, false);

        SingleMessage currentMessage = (SingleMessage)super.getItem(position);

        TextView phoneNumber = (TextView)formattedMessage.findViewById(R.id.PhoneNumber);
        phoneNumber.setText(currentMessage.getPhoneNumber());

        TextView date = (TextView)formattedMessage.findViewById(R.id.Date);
        date.setText(currentMessage.getDate());

        TextView time = (TextView)formattedMessage.findViewById(R.id.Time);
        time.setText(currentMessage.getTime());

        TextView messageBody = (TextView)formattedMessage.findViewById(R.id.MessageBody);
        messageBody.setText(currentMessage.getMessageContent());

        return formattedMessage;
    }
}
