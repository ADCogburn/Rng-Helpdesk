import { Heart, Swords, Trophy, Users, type LucideIcon } from 'lucide-react'
import { Card } from '@/components/ui'
import { clan } from '@/config/clan'
import { SectionHeading } from './SectionHeading'

type FeatureIcon = (typeof clan.about.features)[number]['icon']
const icons: Record<FeatureIcon, LucideIcon> = {
  users: Users,
  swords: Swords,
  trophy: Trophy,
  heart: Heart,
}

export function AboutSection() {
  return (
    <section id="about" className="mx-auto max-w-6xl scroll-mt-20 px-4 py-16 sm:px-6 sm:py-20">
      <SectionHeading
        eyebrow="Who we are"
        title={clan.about.heading}
        description={clan.about.intro}
      />
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {clan.about.features.map((f) => {
          const Icon = icons[f.icon]
          return (
            <li key={f.title} className="contents">
              <Card interactive className="h-full">
                <span className="bg-primary/10 text-primary mb-4 grid size-11 place-items-center rounded-lg">
                  <Icon aria-hidden className="size-6" />
                </span>
                <h3 className="text-lg font-semibold">{f.title}</h3>
                <p className="text-muted mt-2 text-sm leading-relaxed">{f.body}</p>
              </Card>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
