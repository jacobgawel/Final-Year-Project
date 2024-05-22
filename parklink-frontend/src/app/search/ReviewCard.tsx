import { Review } from '@/types/review';
import Rating from '@mui/material/Rating';

export default function ReviewCard({ review }: { review: Review }) {
  return (
    <div className="bg-slate-200 m-5 p-3 rounded shadow-md">
      <div className="">
        <div className="">
            <div className="text-xs font-semibold">
                1 hour ago
            </div>
            <div className="font-semibold">
                {review.reviewTitle} 
            </div>
        </div>
        <div className="">
          <div className="">
            <Rating value={review.reviewRating} readOnly />
          </div>
        </div>
      </div>
        <div className="bg-slate-300 p-2 rounded">
            <div className="font-semibold text-md">
                {review.reviewText}
            </div>
            <div className="text-xs font-semibold">
                Review Date: {review.createdAt}
            </div>
        </div>
    </div>
  );
}