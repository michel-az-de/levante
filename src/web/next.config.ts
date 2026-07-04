import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  // Runtime enxuto para container (server.js + .next/standalone). Ver src/web/Dockerfile.
  output: "standalone",
};

export default nextConfig;
