import {ImageResponse} from "next/og";

export const runtime = "edge";
export const size = {width: 32, height: 32};
export const contentType = "image/png";

export default function Icon() {
  return new ImageResponse(
    (
      <div
        style={{
          fontSize: 20, // 少し小さめにして余白を持たせる
          background: "#0F172A", // 背景色
          width: "100%",
          height: "100%",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: "white", // 文字色
          borderRadius: "0px", // Morgan Stanleyのような「角」のある四角形
          // もし少し丸くしたいなら "4px" にしてください
          fontFamily: '"Times New Roman", Times, serif', // ロゴに合わせてSerif体
          fontWeight: 600,
        }}
      >
        A
      </div>
    ),
    {...size}
  );
}
