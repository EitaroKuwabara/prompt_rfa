# web_api/aps_utils.py
"""
APSのAPIを使用して、Cloud Generationを行うためのユーティリティ関数
(修正版: Direct-to-S3 アップロード対応)
"""
import os
import time
import urllib.parse
import requests
from dotenv import load_dotenv

load_dotenv()

CLIENT_ID = os.getenv("APS_CLIENT_ID")
CLIENT_SECRET = os.getenv("APS_CLIENT_SECRET")
ACTIVITY_NAME = os.getenv("APS_ACTIVITY_NAME")
ACTIVITY_ID = f"{CLIENT_ID}.{ACTIVITY_NAME}+dev"

BASE_URL = "https://developer.api.autodesk.com"
if CLIENT_ID is None:
    raise ValueError("CLIENT_ID is not set")

BUCKET_KEY = f"{CLIENT_ID.lower()}-transient-bucket"
TIMEOUT = 30
UPLOAD_TIMEOUT = 120  # アップロードは時間がかかる場合があるので長めに


def get_token():
    """認証トークンを取得"""
    url = f"{BASE_URL}/authentication/v2/token"
    data = {
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
        "grant_type": "client_credentials",
        "scope": "code:all data:write data:read bucket:create bucket:read",
    }
    res = requests.post(url, data=data, timeout=TIMEOUT)
    res.raise_for_status()
    return res.json()["access_token"]


def ensure_bucket_exists(token):
    """バケットが存在するか確認"""
    url = f"{BASE_URL}/oss/v2/buckets"
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    payload = {"bucketKey": BUCKET_KEY, "policyKey": "transient"}

    res = requests.post(url, headers=headers, json=payload, timeout=TIMEOUT)
    if res.status_code == 409:
        return  # 既に存在するのでOK
    elif res.status_code != 200:
        print(f"Bucket Check Warning: {res.status_code} {res.text}")


def upload_file_to_oss(token, file_path):
    """
    【重要】Direct-to-S3 方式でのアップロード (3ステップ)
    アップロード後、OSSオブジェクトのファイル名を返す（署名付きURLではなくOSS URLで参照するため）
    """
    ensure_bucket_exists(token)
    filename = os.path.basename(file_path)
    safe_filename = urllib.parse.quote(filename)

    # 1. 署名付きアップロードURLを取得 (uploadKeyとurlsをもらう)
    endpoint = (
        f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{safe_filename}/signeds3upload"
    )
    headers = {"Authorization": f"Bearer {token}"}
    res_get = requests.get(endpoint, headers=headers, timeout=TIMEOUT)
    res_get.raise_for_status()

    data = res_get.json()
    upload_key = data["uploadKey"]
    upload_url = data["urls"][0]  # 最初のURLを使用

    # 2. バイナリデータをS3に直接PUT (ここはAuthorizationヘッダー不要！)
    with open(file_path, "rb") as f:
        # S3への直接アップロードはAutodeskのトークンを使わない
        res_upload = requests.put(upload_url, data=f, timeout=UPLOAD_TIMEOUT)
        res_upload.raise_for_status()

    # 3. アップロード完了通知 (Complete Upload)
    # これを呼ばないとOSS側でファイルが有効にならない
    complete_body = {"uploadKey": upload_key}
    headers_complete = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
    }
    res_complete = requests.post(
        endpoint, headers=headers_complete, json=complete_body, timeout=TIMEOUT
    )
    res_complete.raise_for_status()

    # ファイル名を返す（呼び出し元でOSS URLを構築する）
    return filename


def get_download_url(token, object_key):
    """ダウンロード用URLを取得"""
    ensure_bucket_exists(token)
    safe_key = urllib.parse.quote(object_key)
    url = f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{safe_key}/signeds3download"
    res = requests.get(
        url, headers={"Authorization": f"Bearer {token}"}, timeout=TIMEOUT
    )
    res.raise_for_status()
    return res.json()["url"]


def get_upload_params_for_output(token, filename):
    """
    出力ファイル用の「書き込み先URL」と「完了キー」を取得
    """
    ensure_bucket_exists(token)
    safe_filename = urllib.parse.quote(filename)
    endpoint = (
        f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{safe_filename}/signeds3upload"
    )
    headers = {"Authorization": f"Bearer {token}"}

    res = requests.get(endpoint, headers=headers, timeout=TIMEOUT)
    res.raise_for_status()

    data = res.json()
    return data["urls"][0], data["uploadKey"]  # URLとキーの両方を返す


def complete_upload_for_output(token, filename, upload_key):
    """
    クラウド処理完了後に、出力ファイルを有効化する
    """
    safe_filename = urllib.parse.quote(filename)
    endpoint = (
        f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{safe_filename}/signeds3upload"
    )

    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    body = {"uploadKey": upload_key}

    res = requests.post(endpoint, headers=headers, json=body, timeout=TIMEOUT)
    res.raise_for_status()


def run_gen_on_cloud(json_path, template_path, output_path):
    """Cloud Generationを実行"""
    print("--- Cloud Generation Started (New Flow) ---")
    token = get_token()

    # 1. 入力ファイルのアップロード (新方式)
    print("Uploading inputs...")
    rvt_filename = upload_file_to_oss(token, template_path)
    json_filename = upload_file_to_oss(token, json_path)

    # 2. 出力先の準備 (URLとキーを取得)
    output_filename = f"output_{int(time.time())}.rfa"
    # ここで uploadKey も受け取るのが重要
    result_put_url, result_upload_key = get_upload_params_for_output(
        token, output_filename
    )

    print("Starting WorkItem...")
    workitem_url = f"{BASE_URL}/da/us-east/v3/workitems"
    auth_header = {"Authorization": f"Bearer {token}"}
    workitem_args = {
        "activityId": ACTIVITY_ID,
        "arguments": {
            "rvtFile": {
                "url": f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{urllib.parse.quote(rvt_filename)}",
                "headers": auth_header,
                "verb": "get",
            },
            "inputJson": {
                "url": f"{BASE_URL}/oss/v2/buckets/{BUCKET_KEY}/objects/{urllib.parse.quote(json_filename)}",
                "headers": auth_header,
                "verb": "get",
            },
            "resultRfa": {"url": result_put_url, "verb": "put"},
        },
    }
    res = requests.post(
        workitem_url,
        headers={"Authorization": f"Bearer {token}"},
        json=workitem_args,
        timeout=TIMEOUT,
    )

    if res.status_code != 200:
        print(f"WorkItem Start Failed: {res.text}")
    res.raise_for_status()

    workitem_id = res.json()["id"]
    print(f"WorkItem ID: {workitem_id}")

    # 3. 完了待機
    while True:
        time.sleep(5)
        status_url = f"{workitem_url}/{workitem_id}"
        res_status = requests.get(
            status_url, headers={"Authorization": f"Bearer {token}"}, timeout=TIMEOUT
        )
        status_data = res_status.json()
        status = status_data["status"]
        print(f"Status: {status}")

        if status == "success":
            report_url = status_data.get("reportUrl")
            print(f"📝 Autodesk Report Log: {report_url}")
            break
        if status in ["failed", "cancelled", "failedInstructions", "failedDownload", "failedUpload"]:
            print("Job Failed!")
            report_url = status_data.get("reportUrl")
            if report_url:
                print("Report Log:", report_url)
            raise RuntimeError("Cloud processing failed.")

    # 4. 【重要】出力ファイルの完了通知を送る
    # これをやらないと、次のダウンロードで「ファイルがない」と言われる
    print("Finalizing Output File...")
    complete_upload_for_output(token, output_filename, result_upload_key)

    # 5. ダウンロード
    print("Downloading Result...")
    download_url = get_download_url(token, output_filename)

    res_file = requests.get(download_url, timeout=UPLOAD_TIMEOUT)
    with open(output_path, "wb") as f:
        f.write(res_file.content)

    print(f"Saved to: {output_path}")
    return output_path
