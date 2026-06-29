import { ImageResponse } from "next/og";
import { site } from "@/lib/site";

export const size = { width: 1200, height: 630 };
export const contentType = "image/png";
export const alt = site.nome;

export default function OpenGraphImage() {
  return new ImageResponse(
    (
      <div
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          padding: 96,
          background: "#0a0a0a",
          color: "#fafafa",
        }}
      >
        <div style={{ fontSize: 80, fontWeight: 700 }}>{site.nome}</div>
        <div style={{ fontSize: 34, marginTop: 24, color: "#a3a3a3" }}>
          Da pedra bruta a pedra polida
        </div>
      </div>
    ),
    size,
  );
}
