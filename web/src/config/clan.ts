// Clan identity. Placeholder copy -- fill in before launch.
export const clan = {
  name: 'RNG',
  tagline:
    'A Old School RuneScape clan built on good company, grinding together and a little luck.',
  discordInviteUrl: 'https://discord.gg/your-invite-here',
  about: {
    heading: 'About the clan',
    intro:
      'We are a friendly Old School RuneScape community. Whether you are chasing your first raid or your hundredth, there is always someone online to join you.',
    features: [
      {
        icon: 'users',
        title: 'Friendly community',
        body: 'New and veteran players alike. Ask questions, find a team, share the drops.',
      },
      {
        icon: 'swords',
        title: 'Regular events',
        body: 'Bossing nights, skilling competitions and clan-wide challenges.',
      },
      {
        icon: 'trophy',
        title: 'Earn your rank',
        body: 'Clan points for participation climb a 14-tier ladder from Bronze to Zenyte.',
      },
      {
        icon: 'heart',
        title: 'Zero pressure',
        body: 'Play at your own pace. We value people over grind.',
      },
    ],
  },
  joinSteps: [
    {
      title: 'Join our Discord',
      body: 'Use the invite link and say hello in the welcome channel.',
    },
    {
      title: 'Share your RSN',
      body: 'Tell an admin your RuneScape name so we can link your account.',
    },
    { title: 'Start earning points', body: 'Take part in events and watch your rank climb.' },
  ],
} as const
