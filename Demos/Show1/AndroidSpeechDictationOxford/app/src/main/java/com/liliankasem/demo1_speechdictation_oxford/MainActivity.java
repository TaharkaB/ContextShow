package com.liliankasem.demo1_speechdictation_oxford;

import android.app.Activity;
import android.os.Bundle;
import android.widget.EditText;
import android.app.AlertDialog;

import com.microsoft.projectoxford.speechrecognition.ISpeechRecognitionServerEvents;
import com.microsoft.projectoxford.speechrecognition.MicrophoneRecognitionClient;
import com.microsoft.projectoxford.speechrecognition.MicrophoneRecognitionClientWithIntent;
import com.microsoft.projectoxford.speechrecognition.RecognitionResult;
import com.microsoft.projectoxford.speechrecognition.RecognitionStatus;
import com.microsoft.projectoxford.speechrecognition.SpeechRecognitionMode;
import com.microsoft.projectoxford.speechrecognition.SpeechRecognitionServiceFactory;

public class MainActivity extends Activity implements ISpeechRecognitionServerEvents
{
    MicrophoneRecognitionClient m_micClient = null;
    boolean m_isMicrophoneReco;
    SpeechRecognitionMode m_recoMode;
    boolean m_isIntent;
    FinalResponseStatus isReceivedResponse = FinalResponseStatus.NotReceived;
    String dictationtext = "";

    public enum FinalResponseStatus { NotReceived, OK, Timeout }

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        if (getString(R.string.subscription_key).startsWith("Please")) {
            new AlertDialog.Builder(this)
                    .setTitle(getString(R.string.add_subscription_key_tip_title))
                    .setMessage(getString(R.string.add_subscription_key_tip))
                    .setCancelable(false)
                    .show();
        }

        // m_recoMode can be SpeechRecognitionMode.ShortPhrase or SpeechRecognitionMode.LongDictation
        m_recoMode = SpeechRecognitionMode.LongDictation;
        m_isMicrophoneReco = true;

        //Set this to 'true' to use LUIS
        //I haven't actaully created and trained a model so this won't work right now
        m_isIntent = false;

        initializeRecoClient();

        if (m_isMicrophoneReco) {
            m_micClient.startMicAndRecognition();
        }
    }


    /* Speech Recognition using Project Oxford */


    void initializeRecoClient()
    {
        String language = "en-gb";

        String subscriptionKey = this.getString(R.string.subscription_key);
        String luisAppID = this.getString(R.string.luisAppID);
        String luisSubscriptionID = this.getString(R.string.luisSubscriptionID);

        if (m_isMicrophoneReco && null == m_micClient) {
            if (!m_isIntent) {
                m_micClient = SpeechRecognitionServiceFactory.createMicrophoneClient(this,
                        m_recoMode,
                        language,
                        this,
                        subscriptionKey);
            }
            else {
                //Using LUIS
                MicrophoneRecognitionClientWithIntent intentMicClient;
                intentMicClient = SpeechRecognitionServiceFactory.createMicrophoneClientWithIntent(this,
                        language,
                        this,
                        subscriptionKey,
                        luisAppID,
                        luisSubscriptionID);
                m_micClient = intentMicClient;
            }
        }
    }


    public void onPartialResponseReceived(final String response)
    {
        EditText myEditText = (EditText) findViewById(R.id.editText1);
        myEditText.setText(dictationtext + " " + response + " ");
    }

    public void onFinalResponseReceived(final RecognitionResult response)
    {
        boolean isFinalDicationMessage = m_recoMode == SpeechRecognitionMode.LongDictation &&
                (response.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                        response.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout);

        if (response.Results.length > 0) {
            dictationtext +=  response.Results[0].DisplayText;
            EditText myEditText = (EditText) findViewById(R.id.editText1);
            myEditText.setText(" " + dictationtext + " ");
        }


        if (m_isMicrophoneReco && ((m_recoMode == SpeechRecognitionMode.ShortPhrase) || isFinalDicationMessage)) {
            // we got the final result, so it we can end the mic reco.  No need to do this
            // for dataReco, since we already called endAudio() on it as soon as we were done
            // sending all the data.
            m_micClient.endMicAndRecognition();
        }

        if ((m_recoMode == SpeechRecognitionMode.ShortPhrase) || isFinalDicationMessage) {
            this.isReceivedResponse = FinalResponseStatus.OK;
        }
    }

    /**
     * only in ShortPhrase mode + using LUIS
     *
     * Called when a final response is received and its intent is parsed
     */
    public void onIntentReceived(final String payload)
    {
        EditText myEditText = (EditText) findViewById(R.id.editText1);
        myEditText.append("\n********* Final Intent *********\n");
        myEditText.append(payload + "\n");
    }

    public void onError(final int errorCode, final String response)
    {
        EditText myEditText = (EditText) findViewById(R.id.editText1);
        myEditText.append("\n********* Error Detected *********\n");
        myEditText.append(errorCode + " " + response + "\n");
    }

    /**
     * Invoked when the audio recording state has changed.
     *
     * @param recording The current recording state
     */
    public void onAudioEvent(boolean recording)
    {
        if (!recording) {
            m_micClient.endMicAndRecognition();
        }
    }

}