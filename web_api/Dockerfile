# 軽量なPython 3.13イメージを使用
FROM python:3.13-slim

# 作業ディレクトリを設定
WORKDIR /app

# 依存関係ファイルをコピーしてインストール
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# ソースコード全体をコピー
COPY . .

# Railwayは環境変数PORTを提供するため、それを使って起動
# (シェルの機能を使うため、Exec形式["..."]ではなくShell形式で記述)
CMD uvicorn main:app --host 0.0.0.0 --port $PORT