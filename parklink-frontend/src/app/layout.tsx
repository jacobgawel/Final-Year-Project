import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import NavigationBar from './components/Navbar'
import { Toaster } from 'sonner'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'Parklink - Find parking in your city',
  description: 'Parklink is a new way to find parking in your city.',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {

  var year = new Date().getFullYear().toString()

  return (
    <html lang="en">
      <body className={inter.className}>
        <Toaster richColors />
        <NavigationBar />
        <main className="container mx-auto py-10">
            {children}
        </main>
        <footer className="bg-white rounded-lg shadow m-4 dark:bg-gray-800">
            <div className="w-full mx-auto max-w-screen-xl p-4 md:flex md:items-center md:justify-between">
              <span className="text-sm text-gray-500 sm:text-center dark:text-gray-400">© {year} Parklink™. All Rights Reserved.
            </span>
            </div>
        </footer>
      </body>
    </html>
  )
}
