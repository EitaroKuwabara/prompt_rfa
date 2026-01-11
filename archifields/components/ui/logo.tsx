// archifields/components/ui/logo.tsx

export const Logo = ({className}: {className?: string}) => {
  return (
    <div className={`flex items-center ${className}`}>
      <span
        style={{
          fontFamily: '"Times New Roman", Times, serif', // セリフ体
          letterSpacing: "-0.03em", // 文字間を少し詰めて引き締める
          fontWeight: 600, // ほどよい太さ
          fontSize: "1.5rem", // サイズ感
          color: "#0F172A", // slate-900 (真っ黒すぎない濃いグレー)
        }}
      >
        Archifields
      </span>
    </div>
  );
};
