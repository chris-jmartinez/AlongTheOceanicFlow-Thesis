import logging
import json
import datetime
import requests
from flask import Flask
from flask import request

port = 7007

log = logging.getLogger('werkzeug')
log.setLevel(logging.ERROR)

app = Flask(__name__)


@app.route("/", methods=['GET', 'POST'])
def index():
    if request.method == 'POST':
        if request.is_json:
            # try-catch to check json format
            try:
                # Get Json and convert to pure string, no u'
                dataStr = json.dumps(request.get_json())
                responseStr = "{\"status\":\"ok\"}"
            except:
                dataStr = "A request with no-json format is arrived"
                responseStr = dataStr
        else:
            dataStr = "A request with no \"Content-Type: application/json\" header is arrived"
            responseStr = dataStr

        # This complex and heavy functions to generate a date formatted like:
        # 2018-11-15 12:53:51.606
        (datetimeStr, micro) = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f').split('.')
        datetimeStr = "%s.%03d" % (datetimeStr, round(float(micro) / 1000.0))

        payload = {'key1': 'value1', 'key2': 'value2'}
        print(dataStr)
        r = requests.post("http://192.168.43.183:7007", json=dataStr)
        #print("Response :"+ r)
        print(datetimeStr + " " + dataStr)
        return responseStr
    else:
        return "You shouldn't use get method!"



if __name__ == "__main__":
    print("Starting web server at port:" + str(port))
    app.run(host="0.0.0.0", port=port)