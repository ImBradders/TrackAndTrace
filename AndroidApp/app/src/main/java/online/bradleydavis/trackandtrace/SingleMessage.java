package online.bradleydavis.trackandtrace;

public class SingleMessage {
    String phoneNumber;
    String date;
    String time;
    String messageContent;

    public SingleMessage() {
        phoneNumber = "00000000000";
        date = "01/01/1970";
        time = "00:01";
        messageContent = "No message provided";
    }

    public SingleMessage(String phoneNumber, String date, String time, String messageContent) {
        this.phoneNumber = phoneNumber;
        this.date = date;
        this.time = time;
        this.messageContent = messageContent;
    }

    public String getPhoneNumber() {
        return phoneNumber;
    }

    public void setPhoneNumber(String phoneNumber) {
        this.phoneNumber = phoneNumber;
    }

    public String getDate() {
        return date;
    }

    public void setDate(String date) {
        this.date = date;
    }

    public String getTime() {
        return time;
    }

    public void setTime(String time) {
        this.time = time;
    }

    public String getMessageContent() {
        return messageContent;
    }

    public void setMessageContent(String messageContent) {
        this.messageContent = messageContent;
    }
}
