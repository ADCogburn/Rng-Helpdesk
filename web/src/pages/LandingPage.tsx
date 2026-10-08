import {
  AboutSection,
  Hero,
  JoinSection,
  Leaderboard,
  LandingFooter,
  LandingNav,
  RankLadder,
} from '@/components/landing'

export function Component() {
  return (
    <div className="bg-bg min-h-screen">
      <LandingNav />
      <main>
        <Hero />
        <AboutSection />
        <RankLadder />
        <Leaderboard />
        <JoinSection />
      </main>
      <LandingFooter />
    </div>
  )
}
