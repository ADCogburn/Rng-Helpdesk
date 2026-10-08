import { clan } from '@/config/clan'
import { buttonClass } from '@/components/ui/buttonStyles'
import { SectionHeading } from './SectionHeading'

export function JoinSection() {
  return (
    <section id="join" className="bg-surface/40 border-line/60 scroll-mt-20 border-y">
      <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6 sm:py-20">
        <SectionHeading
          eyebrow="Get started"
          title="How to join"
          description="Three steps and you are one of us."
        />
        <ol className="grid gap-4 md:grid-cols-3">
          {clan.joinSteps.map((s, i) => (
            <li
              key={s.title}
              className="border-line bg-surface hover:border-primary/50 relative rounded-xl border p-6 transition hover:-translate-y-0.5"
            >
              <span className="bg-primary text-primary-fg font-display shadow-primary/30 mb-4 grid size-10 place-items-center rounded-full text-lg font-bold shadow-md">
                {i + 1}
              </span>
              <h3 className="text-lg font-semibold">{s.title}</h3>
              <p className="text-muted mt-2 text-sm leading-relaxed">{s.body}</p>
            </li>
          ))}
        </ol>
        <div className="mt-10 text-center">
          <a
            href={clan.discordInviteUrl}
            target="_blank"
            rel="noreferrer"
            className={buttonClass('primary', 'lg')}
          >
            Join our Discord
          </a>
        </div>
      </div>
    </section>
  )
}
