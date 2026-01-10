# setup_aps.py
"""
登録用スクリプト
"""
import os
import sys
import requests
from dotenv import load_dotenv

# .envを読み込む
load_dotenv()

CLIENT_ID = os.getenv("APS_CLIENT_ID")
CLIENT_SECRET = os.getenv("APS_CLIENT_SECRET")
APPBUNDLE_NAME = os.getenv("APS_APPBUNDLE_NAME")
ACTIVITY_NAME = os.getenv("APS_ACTIVITY_NAME")
# ZIPファイルのパス (デスクトップ等にある場合はパスを修正してください。ここではルートにあると仮定)
ZIP_PATH = "PromptRFA.bundle.zip"

BASE_URL = "https://developer.api.autodesk.com"
ENGINE_ID = "Autodesk.Revit+2025"  # Revit 2025エンジンを使用

TIMEOUT = 30
UPLOAD_TIMEOUT = 60


def get_token():
    """認証トークンを取得"""
    url = f"{BASE_URL}/authentication/v2/token"
    data = {
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
        "grant_type": "client_credentials",
        "scope": "code:all data:write data:read bucket:create bucket:read",
    }
    res = requests.post(
        url,
        data=data,
        timeout=TIMEOUT,
    )
    if res.status_code != 200:
        print("Auth Error:", res.text)
        sys.exit(1)
    return res.json()["access_token"]


def create_appbundle(auth_token):
    """AppBundle (ZIP) を登録"""
    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Content-Type": "application/json",
    }

    # 1. AppBundleの定義を作成
    appbundle_id = f"{CLIENT_ID}.{APPBUNDLE_NAME}+dev"
    print(f"Creating AppBundle: {appbundle_id} ...")

    url = f"{BASE_URL}/da/us-east/v3/appbundles"
    spec = {"id": APPBUNDLE_NAME, "engine": ENGINE_ID}

    # 既に存在する場合は削除するか、バージョンを上げる必要があるが、今回は「上書き(Delete -> Create)」方針で
    # 本来はAliasを使うが、簡易化のため削除リクエストを試みる
    requests.delete(
        f"{url}/{APPBUNDLE_NAME}",
        headers=headers,
        timeout=TIMEOUT,
    )

    # 新規作成リクエスト
    res = requests.post(
        url,
        headers=headers,
        json=spec,
        timeout=TIMEOUT,
    )

    if res.status_code == 200:
        data = res.json()
        upload_params = data["uploadParameters"]
        endpoint_url = upload_params["endpointURL"]
        form_data = upload_params["formData"]

        print("Uploading ZIP file...")
        # 2. ZIPファイルをアップロード
        with open(ZIP_PATH, "rb") as f:
            files = {"file": f}
            # formDataを含めてPOST
            res_upload = requests.post(
                endpoint_url,
                data=form_data,
                files=files,
                timeout=UPLOAD_TIMEOUT,
            )
            if res_upload.status_code != 200:
                print("Upload Failed:", res_upload.text)
                return None

        print("AppBundle Created & Uploaded!")

        # 3. エイリアス (dev) を作成/更新
        alias_url = f"{url}/{APPBUNDLE_NAME}/aliases"
        alias_data = {"id": "dev", "version": 1}
        # エイリアス作成(POST)または更新(PATCH)
        res_alias = requests.post(
            alias_url,
            headers=headers,
            json=alias_data,
            timeout=UPLOAD_TIMEOUT,
        )
        if res_alias.status_code == 409:  # 既にのエイリアスがある
            requests.patch(
                f"{alias_url}/dev",
                headers=headers,
                json=alias_data,
                timeout=UPLOAD_TIMEOUT,
            )

        return f"{CLIENT_ID}.{APPBUNDLE_NAME}+dev"

    else:
        print("Create AppBundle Failed:", res.text)
        return None


def create_activity(auth_token, appbundle_id):
    """Activity (処理の定義) を作成"""
    print(f"Creating Activity: {ACTIVITY_NAME} ...")
    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Content-Type": "application/json",
    }
    url = f"{BASE_URL}/da/us-east/v3/activities"

    # 既存削除
    requests.delete(
        f"{url}/{ACTIVITY_NAME}",
        headers=headers,
        timeout=TIMEOUT,
    )

    # コマンドライン引数の定義
    # JSONファイルを読み込み、Revitエンジンで処理し、結果を保存する
    command_line = [
        f'$(engine.path)\\\\revitcoreconsole.exe /i "$(args[rvtFile].path)" '
        f'/al "$(appbundles[{APPBUNDLE_NAME}].path)"'
    ]

    spec = {
        "id": ACTIVITY_NAME,
        "commandLine": command_line,
        "parameters": {
            "rvtFile": {
                "verb": "get",
                "description": "Input Revit File",
                "required": True,
                "localName": "$(eng.path)\\\\RevitDoc.rvt",  # ダミーでも必要
            },
            "inputJson": {
                "verb": "get",
                "description": "Input JSON parameters",
                "required": True,
                "localName": "components.json",
            },
            "resultRfa": {
                "verb": "put",
                "description": "Output RFA Family",
                "required": True,
                "localName": "output.rfa",  # FamilyProcessor.cs で保存する名前に合わせる必要あり(後述)
            },
        },
        "engine": ENGINE_ID,
        "appbundles": [appbundle_id],
    }

    res = requests.post(
        url,
        headers=headers,
        json=spec,
        timeout=TIMEOUT,
    )
    if res.status_code == 200:
        print("Activity Created!")
        # エイリアス作成
        alias_url = f"{url}/{ACTIVITY_NAME}/aliases"
        alias_data = {"id": "dev", "version": 1}
        res_alias = requests.post(
            alias_url,
            headers=headers,
            json=alias_data,
            timeout=UPLOAD_TIMEOUT,
        )
        if res_alias.status_code == 409:
            requests.patch(
                f"{alias_url}/dev",
                headers=headers,
                json=alias_data,
                timeout=UPLOAD_TIMEOUT,
            )

        print("Setup Complete!")
    else:
        print("Create Activity Failed:", res.text)


if __name__ == "__main__":
    if not os.path.exists(ZIP_PATH):
        print(f"Error: {ZIP_PATH} not found. Please place the zip file in this folder.")
    else:
        token = get_token()
        ab_id = create_appbundle(token)
        if ab_id:
            create_activity(token, ab_id)
