export const queryKeys = {
  auth: { me: ['auth', 'me'] as const },
  public: {
    overview: ['public', 'overview'] as const,
    ranks: ['public', 'ranks'] as const,
    leaderboard: (top: number) => ['public', 'leaderboard', top] as const,
  },
  users: {
    all: ['users'] as const,
    list: ['users', 'list'] as const,
    detail: (id: string) => ['users', 'detail', id] as const,
    byRsn: (rsn: string) => ['users', 'by-rsn', rsn] as const,
    byHistoricalRsn: (rsn: string) => ['users', 'by-historical-rsn', rsn] as const,
    lifecycle: (id: string) => ['users', id, 'lifecycle'] as const,
    pointHistory: (id: string) => ['users', id, 'point-history'] as const,
    rsAccounts: (id: string) => ['users', id, 'rs-accounts'] as const,
    rsPrevious: (id: string) => ['users', id, 'rs-accounts', 'previous'] as const,
    rsHistory: (id: string) => ['users', id, 'rs-accounts', 'history'] as const,
  },
  rankThresholds: ['rank-thresholds'] as const,
}
