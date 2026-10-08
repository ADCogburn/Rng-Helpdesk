import '@testing-library/jest-dom/vitest'

// jsdom does not implement scrolling (used by React Router's <ScrollRestoration>).
window.scrollTo = () => {}
